using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using StudentExercises.Data;
using Dapper;
using Microsoft.AspNetCore.Http;

namespace StudentExercises.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StudentsController(IConfiguration config)
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

        // GET /students
        [HttpGet]
        public async Task<IActionResult> Get(string q, int? cohort)
        {
            string sql = @"
            SELECT
                s.Id,
                s.FirstName,
                s.LastName,
                s.SlackHandle,
                s.CohortId,
                c.Id,
                c.Name
            FROM Student s
            JOIN Cohort c ON s.CohortId = c.Id
            WHERE 1=1
            ";

            if (q != null)
            {
                string isQ = $@"
                    AND (s.FirstName LIKE '%{q}%'
                    OR s.LastName LIKE '%{q}%'
                    OR s.SlackHandle LIKE '%{q}%')
                ";
                sql = $"{sql} {isQ}";
            }

            if (cohort != null)
            {
                string inCohort = $@"
                    AND (c.Name LIKE '%{cohort}%')
                ";
                sql = $"{sql} {inCohort}";
            }

            Console.WriteLine(sql);

            using (IDbConnection conn = Connection)
            {

                IEnumerable<Student> students = await conn.QueryAsync<Student, Cohort, Student>(
                    sql,
                    (student, cohrt) =>
                    {
                        student.Cohort = cohrt;
                        return student;
                    }
                );
                return Ok(students);
            }
        }

        // GET /students/5
        [HttpGet("{id}", Name = "GetStudent")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $@"
            SELECT
                s.Id,
                s.FirstName,
                s.LastName,
                s.SlackHandle,
                s.CohortId
            FROM Student s
            WHERE s.Id = {id}
            ";

            using (IDbConnection conn = Connection)
            {
                IEnumerable<Student> students = await conn.QueryAsync<Student>(sql);
                return Ok(students);
            }
        }

        // POST /students
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Student student)
        {
            string sql = $@"INSERT INTO Student
            (FirstName, LastName, SlackHandle, CohortId)
            VALUES
            (
                '{student.FirstName}'
                ,'{student.LastName}'
                ,'{student.SlackHandle}'
                ,'{student.CohortId}'
            );
            select seq from sqlite_sequence where name='Student';";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                student.Id = newId;
                return CreatedAtRoute("GetStudent", new { id = newId }, student);
            }
        }

        // PUT /students/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Student student)
        {
            string sql = $@"
            UPDATE Student
            SET FirstName = '{student.FirstName}',
                LastName = '{student.LastName}',
                SlackHandle = '{student.SlackHandle}'
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
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE /students/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string sql = $@"DELETE FROM Student WHERE Id = {id}";

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

        private bool StudentExists(int id)
        {
            string sql = $"SELECT Id FROM Student WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<Student>(sql).Count() > 0;
            }
        }
    }
}
