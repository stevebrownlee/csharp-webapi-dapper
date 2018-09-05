using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NSSWeb.Data;
using Dapper;
using Microsoft.AspNetCore.Http;

namespace NSSWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExercisesController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ExercisesController(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection Connection
        {
            get
            {
                return new SqliteConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        // GET api/exercises
        [HttpGet]
        public async Task<IActionResult> Get(string q, string _include, string language)
        {
            // Store URL parameters in a tuple
            (string q, string include, string language) filter = (q, _include, language);

            string sqlSelect = "SELECT e.Id, e.Name, e.Language";
            string sqlFrom = "FROM Exercise e";
            string sqlJoin = "";
            string sqlWhere = "WHERE 1=1";

            string isQ = $"AND (e.Language LIKE '%{q}%' OR e.Name LIKE '%{q}%')";
            string isLanguage = $"AND e.Language = '{filter.language}'";

            string sqlSelectStudents = @"
                       ,s.Id
                       ,s.FirstName
                       ,s.LastName
                       ,s.SlackHandle";
            string studentsIncluded = "JOIN Student s ON se.StudentId = s.Id";
            string studentsExerciseIncluded = "JOIN StudentExercise se ON e.Id = se.ExerciseId";


            string sqlSelectInstructors = @"
                       ,i.Id
                       ,i.FirstName
                       ,i.LastName
                       ,i.Specialty
                       ,i.SlackHandle";
            string instructorsIncluded = "JOIN Instructor i ON i.Id = se.InstructorId";

            if (filter.include.Contains("students"))
            {
                sqlSelect = $@"{sqlSelect} {sqlSelectStudents}";
                sqlJoin = $"{sqlJoin} {studentsExerciseIncluded} {studentsIncluded}";
            }

            if (filter.include.Contains("instructors"))
            {
                sqlSelect = $@"{sqlSelect} {sqlSelectInstructors}";
                sqlJoin = $"{sqlJoin} {studentsExerciseIncluded} {instructorsIncluded}";
            }

            if (filter.q != null)
            {
                sqlWhere = $"{sqlWhere} {isQ}";
            }

            if (filter.language != null)
            {
                sqlWhere = $"{sqlWhere} {isLanguage}";
            }

            string sql = $"{sqlSelect} {sqlFrom} {sqlJoin} {sqlWhere}";
            Console.WriteLine(sql);
            using (IDbConnection conn = Connection)
            {
                if (filter.include == "students")
                {
                    Dictionary<int, Exercise> studentExercises = new Dictionary<int, Exercise>();

                    var fullExercises = await conn.QueryAsync<Exercise, Student, Exercise>(
                        sql,
                        (exercise, student) =>
                        {
                            if (!studentExercises.ContainsKey(exercise.Id))
                            {
                                studentExercises[exercise.Id] = exercise;
                            }
                            studentExercises[exercise.Id].AssignedStudents.Add(student);
                            return exercise;
                        }
                    );
                    return Ok(studentExercises.Values);

                }
                IEnumerable<Exercise> exercises = await conn.QueryAsync<Exercise>(sql);
                return Ok(exercises);
            }
        }

        // GET api/exercises/5
        [HttpGet("{id}", Name = "GetExercise")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $"SELECT Id, Name, Language FROM Exercise WHERE Id = {id}";

            using (IDbConnection conn = Connection)
            {
                IEnumerable<Exercise> exercises = await conn.QueryAsync<Exercise>(sql);
                return Ok(exercises);
            }
        }

        // POST api/exercises
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Exercise exercise)
        {
            string sql = $@"INSERT INTO Exercise
            (`Name`, `Language`)
            VALUES
            ('{exercise.Name}', '{exercise.Language}');
            select seq from sqlite_sequence where name='Exercise';";

            using (IDbConnection conn = Connection)
            {
                var newExerciseId = (await conn.QueryAsync<int>(sql)).Single();
                exercise.Id = newExerciseId;
                return CreatedAtRoute("GetExercise", new { id = newExerciseId }, exercise);
            }
        }

        // PUT api/exercises/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Exercise exercise)
        {
            string sql = $@"
            UPDATE Exercise
            SET Name = '{exercise.Name}',
                Language = '{exercise.Language}'
            WHERE Id = {id}";

            try
            {
                using (IDbConnection conn = Connection)
                {
                    int rowsAffected = await conn.ExecuteAsync(sql);
                    if (rowsAffected > 0)
                    {
                        return new StatusCodeResult(StatusCodes.Status204NoContent);
                    }
                    throw new Exception("No rows affected");
                }
            }
            catch (Exception)
            {
                if (!ExerciseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE api/exercises/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string sql = $@"DELETE FROM Exercise WHERE Id = {id}";

            using (IDbConnection conn = Connection)
            {
                int rowsAffected = await conn.ExecuteAsync(sql);
                if (rowsAffected > 0)
                {
                    return new StatusCodeResult(StatusCodes.Status204NoContent);
                }
                throw new Exception("No rows affected");
            }

        }

        private bool ExerciseExists(int id)
        {
            string sql = $"SELECT Id, Name, Language FROM Exercise WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<Exercise>(sql).Count() > 0;
            }
        }
    }
}
