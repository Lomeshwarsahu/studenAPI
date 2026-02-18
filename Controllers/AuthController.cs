using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using tsting_api.Models;

namespace tsting_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public AuthController(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }

        // ✅ LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login(User loginUser)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            string query = @"SELECT * FROM Users 
                             WHERE Username=@Username AND Password=@Password";

            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Username", loginUser.Username);
            cmd.Parameters.AddWithValue("@Password", loginUser.Password);

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return Unauthorized();

            string username = reader["Username"].ToString();
            string role = reader["Role"].ToString();

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(
                    Convert.ToDouble(_config["Jwt:DurationInMinutes"])
                ),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }

        // ✅ REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            // Check if user exists
            string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username=@Username";
            using SqlCommand checkCmd = new SqlCommand(checkQuery, con);
            checkCmd.Parameters.AddWithValue("@Username", user.Username);

            int userExists = (int)await checkCmd.ExecuteScalarAsync();

            if (userExists > 0)
                return BadRequest("Username already exists");

            // Insert new user
            string insertQuery = @"INSERT INTO Users (Username, Password, Role)
                                   VALUES (@Username, @Password, @Role)";

            using SqlCommand insertCmd = new SqlCommand(insertQuery, con);
            insertCmd.Parameters.AddWithValue("@Username", user.Username);
            insertCmd.Parameters.AddWithValue("@Password", user.Password);
            insertCmd.Parameters.AddWithValue("@Role", user.Role ?? "User");

            await insertCmd.ExecuteNonQueryAsync();

            return Ok("User Registered Successfully");
        }
    }
}
