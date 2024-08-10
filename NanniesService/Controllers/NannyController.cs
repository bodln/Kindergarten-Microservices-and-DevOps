using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NanniesService.Repositories;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace IdentityManagerServerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NannyController : ControllerBase
    {
        private readonly IUserAccount _userAccount;
        private readonly IConfiguration _configuration;

        public NannyController(IUserAccount userAccount,
            IConfiguration configuration)
        {
            _userAccount = userAccount;
            _configuration = configuration;
        }

        //[HttpGet("Authorize")]
        //public IResult Auth()
        //{
        //    try
        //    {
        //        if (!HttpContext.User.Identity?.IsAuthenticated ?? false)
        //        {
        //            Console.WriteLine("--> Nginx authorization FAILED... controller");
        //            return Results.Unauthorized();
        //        }

        //        Console.WriteLine("--> Nginx authorization successful... controller");
        //        return Results.Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Exception: {ex.Message}");
        //        return Results.Problem("Internal Server Error");
        //    }
        //}

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserDTO userDTO)
        {
            var response = await _userAccount.CreateAccount(userDTO);
            return Ok(response);
        }

        [HttpPost("LogIn")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var response = await _userAccount.LoginAccount(loginDTO);
            return Ok(response);
        }

        [HttpPost("{studentId:int}/Grade/{grade:int}")]
        [Authorize(Roles = "Nanny")]
        public IActionResult GradeStudent(int studentId, int grade)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var response = _userAccount.GradeStudent(token, studentId, grade);
            return response ? Ok("Event to grade the Student with the id: " + studentId + ", with the the following grade: " + grade + ", has been pushed to the RabbitMQ queue.") : NotFound("Failed to grade student from Nanny controller...");
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var response = _userAccount.GetAll();
            return Ok(response);
        }

        [HttpGet("TestToken")]
        public IActionResult Test(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudiences = _configuration.GetSection("Jwt:Audiences").Get<List<string>>(),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!))
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                return Ok(new
                {
                    Message = "Token is valid",
                    TokenDetails = jwtToken
                });
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { Message = $"Token validation failed: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpDelete("RemoveAllUsers"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveAllUsers()
        {
            var response = await _userAccount.RemoveAllUsers();
            return Ok(response);
        }

        [HttpDelete("RemoveAllRoles"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveAllRoles()
        {
            var response = await _userAccount.RemoveAllRoles();
            return Ok(response);
        }
    }
}
