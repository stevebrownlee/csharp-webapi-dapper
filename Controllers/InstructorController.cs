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
    public class InstructorsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public InstructorsController(IConfiguration config)
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

        // GET api/instructors
        [HttpGet]
        public async Task<IActionResult> Get(string q)
        {
            string sql = @"
            SELECT
                i.Id,
                i.FirstName,
                i.LastName,
                i.SlackHandle,
                i.Specialty,
                c.Id,
                c.Name
            FROM Instructor i
            JOIN Cohort c ON i.CohortId = c.Id
            WHERE 1=1
            ";

            if (q != null)
            {
                string isQ = $@"
                    AND i.FirstName LIKE '%{q}%'
                    OR i.LastName LIKE '%{q}%'
                    OR i.SlackHandle LIKE '%{q}%'
                    OR i.Specialty LIKE '%{q}%'
                ";
                sql = $"{sql} {isQ}";
            }



            Console.WriteLine(sql);
            using (IDbConnection conn = Connection)
            {

                IEnumerable<Instructor> instructors = await conn.QueryAsync<Instructor, Cohort, Instructor>(
                    sql,
                    (instructor, cohort) =>
                    {
                        instructor.Cohort = cohort;
                        return instructor;
                    }
                );
                return Ok(instructors);
            }
        }

        // GET api/instructors/5
        [HttpGet("{id}", Name = "GetInstructor")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $@"
            SELECT
                i.Id,
                i.FirstName,
                i.LastName,
                i.SlackHandle,
                i.Specialty,
                i.CohortId
            FROM Instructor i
            WHERE i.Id = {id}
            ";

            using (IDbConnection conn = Connection)
            {
                IEnumerable<Instructor> instructors = await conn.QueryAsync<Instructor>(sql);
                return Ok(instructors);
            }
        }

        // POST api/instructors
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Instructor instructor)
        {
            string sql = $@"INSERT INTO Instructor VALUES
            (
                null,
                '{instructor.FirstName}'
                ,'{instructor.LastName}'
                ,'{instructor.SlackHandle}'
                ,'{instructor.Specialty}'
            );
            select seq from sqlite_sequence where name='Instructor';";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                instructor.Id = newId;
                return CreatedAtRoute("GetInstructor", new { id = newId }, instructor);
            }
        }

        // PUT api/instructors/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Instructor instructor)
        {
            string sql = $@"
            UPDATE Instructor
            SET FirstName = '{instructor.FirstName}',
                LastName = '{instructor.LastName}',
                SlackHandle = '{instructor.SlackHandle}',
                Specialty = '{instructor.Specialty}'
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
                if (!InstructorExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE api/instructors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string sql = $@"DELETE FROM Instructor WHERE Id = {id}";

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

        private bool InstructorExists(int id)
        {
            string sql = $"SELECT Id FROM Instructor WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<Instructor>(sql).Count() > 0;
            }
        }
    }
}
