using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using tsting_api.Models;

namespace tsting_api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly string _connectionString;

        public StudentsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetStudents()
        {
            var students = new List<Student>();

            using SqlConnection con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            using SqlCommand cmd = new SqlCommand("SELECT * FROM Students", con);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                students.Add(new Student
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = reader["Name"].ToString(),
                    Age = Convert.ToInt32(reader["Age"]),
                    //Subject = reader["Subject"].ToString(),
                    //DOB = Convert.ToDateTime(reader["DOB"])
                });
            }

            return Ok(students);
        }

        // ✅ GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudent(int id)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            string query = "SELECT * FROM Students WHERE Id=@Id";
            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Id", id);

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var student = new Student
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = reader["Name"].ToString(),
                    Age = Convert.ToInt32(reader["Age"]),
                    //Subject = reader["Subject"].ToString(),
                    //DOB = Convert.ToDateTime(reader["DOB"])
                };

                return Ok(student);
            }

            return NotFound();
        }

        // ✅ POST
        [HttpPost]
        public async Task<IActionResult> AddStudent(Student student)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            string query = @"INSERT INTO Students (Name, Age, Subject, DOB)
                             VALUES (@Name, @Age, @Subject, @DOB)";

            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Name", student.Name);
            cmd.Parameters.AddWithValue("@Age", student.Age);
            //cmd.Parameters.AddWithValue("@Subject", student.Subject);
            //cmd.Parameters.AddWithValue("@DOB", student.DOB);

            await cmd.ExecuteNonQueryAsync();

            return Ok(student);
        }

        // ✅ PUT
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, Student student)
        {
            if (id != student.Id)
                return BadRequest();

            using SqlConnection con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            string query = @"UPDATE Students 
                             SET Name=@Name, Age=@Age, Subject=@Subject, DOB=@DOB 
                             WHERE Id=@Id";

            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Name", student.Name);
            cmd.Parameters.AddWithValue("@Age", student.Age);
            //cmd.Parameters.AddWithValue("@Subject", student.Subject);
            //cmd.Parameters.AddWithValue("@DOB", student.DOB);

            int rows = await cmd.ExecuteNonQueryAsync();

            if (rows == 0)
                return NotFound();

            return Ok(student);
        }

        // ✅ DELETE (Admin Role Only)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            string query = "DELETE FROM Students WHERE Id=@Id";
            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Id", id);

            int rows = await cmd.ExecuteNonQueryAsync();

            if (rows == 0)
                return NotFound();

            return Ok("Deleted Successfully");
        }
    }
}
