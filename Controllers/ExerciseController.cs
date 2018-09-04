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
        public async Task<IActionResult> Get(string language)
        {
            string sql = "SELECT Id, Name, Language FROM Exercise";

            // If language query string parameter is specified
            if (language != null)
            {
                sql = $"{sql} WHERE Language = '{language}'";
            }

            using (IDbConnection conn = Connection)
            {
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
                var newURL = CreatedAtRoute("GetExercise", new { id = newExerciseId }, exercise);
                return newURL;
            }
        }

        // PUT api/exercises/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/exercises/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
