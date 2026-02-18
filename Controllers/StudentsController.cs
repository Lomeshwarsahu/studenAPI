using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using studentapis.DTOs;
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
            var students = new List<StudentReadDTO>();

            using SqlConnection con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            using SqlCommand cmd = new SqlCommand("SELECT * FROM Students", con);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                //students.Add(new Student
                //{
                //    Id = Convert.ToInt32(reader["Id"]),
                //    Name = reader["Name"].ToString(),
                //    Age = Convert.ToInt32(reader["Age"]),
                //    //Subject = reader["Subject"].ToString(),
                //    //DOB = Convert.ToDateTime(reader["DOB"])
                //});
                //AddStudent dto 

                students.Add(new StudentReadDTO
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = reader["Name"].ToString(),
                    Age = Convert.ToInt32(reader["Age"]),


                });
            }

            return Ok(students);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudent(int id)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            string query = "SELECT Id, Name, Email, Age FROM Students WHERE Id=@Id";
            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Id", id);

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var studentDTO = new StudentReadDTO
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = reader["Name"].ToString(),
                    Email = reader["Email"].ToString(),
                    Age = Convert.ToInt32(reader["Age"]),
                };

                return Ok(studentDTO);
            }

            return NotFound("Student not found");
        }


        // ✅ POST
        [HttpPost]
        public async Task<IActionResult> AddStudent(StudentCreateDTO dto)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            string query = @"INSERT INTO Students (Name,Email, Age)
                             VALUES (@Name,@Email, @Age)";

            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Name", dto.Name);
            cmd.Parameters.AddWithValue("@Email", dto.Email);
            cmd.Parameters.AddWithValue("@Age", dto.Age ?? (object)DBNull.Value);
            //cmd.Parameters.AddWithValue("@DOB", student.DOB);, @Subject, @DOB , Subject, DOB

            await cmd.ExecuteNonQueryAsync();

            return Ok(dto);
        }

        // ✅ PUT
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, StudentUpdateDTO dto)
        {
            if (id != dto.Id)
                return BadRequest();

            using SqlConnection con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            string query = @"UPDATE Students 
                             SET Name=@Name, Email=@Email ,Age=@Age
                             WHERE Id=@Id";
           //, Subject = @Subject, DOB = @DOB
            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Name", dto.Name);
            cmd.Parameters.AddWithValue("@Email", dto.Email);
            cmd.Parameters.AddWithValue("@Age", dto.Age);
            //cmd.Parameters.AddWithValue("@DOB", student.DOB);

            int rows = await cmd.ExecuteNonQueryAsync();

            if (rows == 0)
                return NotFound();

            return Ok(dto);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                using SqlConnection con = new SqlConnection(_connectionString);
                await con.OpenAsync();

                string query = "DELETE FROM Students WHERE Id=@Id";
                using SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Id", id);

                int rows = await cmd.ExecuteNonQueryAsync();

                if (rows == 0)
                    return NotFound(new { Message = "Student not found", StudentId = id });

                return Ok(new { Message = "Deleted Successfully", StudentId = id });
            }
            catch (Exception ex)
            {
                // Log error here
                return StatusCode(500, new { Message = "Internal Server Error", Error = ex.Message });
            }
        }

    }
}
