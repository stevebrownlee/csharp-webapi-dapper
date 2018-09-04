﻿using System;
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
        public void Delete(int id)
        {
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