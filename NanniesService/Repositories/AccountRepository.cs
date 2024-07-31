using Azure.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NanniesService.AsyncDataServices;
using NanniesService.Data;
using NanniesService.DTOs;
using NanniesService.Models;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NanniesService.Repositories
{
    public class AccountRepository : IUserAccount
    {
        private readonly UserManager<Nanny> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        private readonly IMessageBusClient _messageBusClient;

        public AccountRepository(UserManager<Nanny> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config,
            AppDbContext context,
            IMessageBusClient messageBusClient)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
            _context = context;
            _messageBusClient = messageBusClient;
        }

        public async Task<string> CreateAccount(UserDTO userDTO)
        {
            var newUser = new Nanny()
            {
                Name = userDTO.Name,
                Email = userDTO.Email,
                PasswordHash = userDTO.Password,
                UserName = userDTO.Email
            };

            var user = await _userManager.FindByEmailAsync(newUser.Email);
            if (user is not null) return "User registered already";

            var createUser = await _userManager.CreateAsync(newUser!, userDTO.Password);
            if (!createUser.Succeeded) return "Error occurred.. please try again";

            var checkAdmin = await _roleManager.FindByNameAsync("Admin");
            if (checkAdmin is null)
            {
                await _roleManager.CreateAsync(new IdentityRole() { Name = "Nanny" });
                await _roleManager.CreateAsync(new IdentityRole() { Name = "Admin" });

                await _userManager.AddToRoleAsync(newUser, "Nanny");
                await _userManager.AddToRoleAsync(newUser, "Admin");
                return "Account Created";
            }
            else
            {
                await _userManager.AddToRoleAsync(newUser, "Nanny");
                return "Account Created";
            }
        }

        public async Task<string> LoginAccount(LoginDTO loginDTO)
        {
            if (loginDTO == null)
                return "Login container is empty";

            var getUser = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (getUser is null)
                return "User not found";

            bool checkUserPasswords = await _userManager.CheckPasswordAsync(getUser, loginDTO.Password);
            if (!checkUserPasswords)
                return "Invalid email/password";

            var getUserRoles = await _userManager.GetRolesAsync(getUser);
            var userSession = new UserSession(getUser.Email, getUserRoles.ToList());
            string token = GenerateToken(userSession);
            return token;
        }

        public List<Nanny> GetAll()
        {
            return _context.Nannies.ToList();
        }

        public async Task<string> RemoveAllUsers()
        {
            var allUsers = _context.Nannies.ToList();
            foreach (var user in allUsers)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return "Failed to delete all users.";
                }
            }
            return "Successfully deleted all users.";
        }

        public async Task<string> RemoveAllRoles()
        {
            var allRoles = _roleManager.Roles.ToList();
            foreach (var role in allRoles)
            {
                var result = await _roleManager.DeleteAsync(role);
                if (!result.Succeeded)
                {
                    return "Failed to delete all roles.";
                }
            }
            return "Successfully deleted all roles.";
        }

        private string GenerateToken(UserSession user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email)
            };
            userClaims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var audiences = _config.GetSection("Jwt:Audiences").Get<List<string>>();
            userClaims.AddRange(audiences.Select(audience => new Claim(JwtRegisteredClaimNames.Aud, audience)));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                claims: userClaims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool GradeStudent(string token, int studentId, int grade)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            string email = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;

            try
            {
                GradeStudentDTO gradeStudentDTO = new GradeStudentDTO
                {
                    studentId = studentId,
                    nannyEmail = email,
                    grade = grade,
                    eventMessage = "Grade_Student",
                    token = token
                };

                _messageBusClient.GradeStudent(gradeStudentDTO);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("--> Failed to send Grade_Student to RabbitMQ Message Bus: " + ex.Message);
                return false;
            }
        }
    }

    public interface IUserAccount
    {
        Task<string> CreateAccount(UserDTO userDTO);
        Task<string> LoginAccount(LoginDTO loginDTO);
        List<Nanny> GetAll();
        Task<string> RemoveAllUsers();
        Task<string> RemoveAllRoles();
        bool GradeStudent(string token, int studentId, int grade);
    }

    public class UserDTO
    {
        public string? Id { get; set; } = string.Empty;
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginDTO
    {
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public record UserSession(string Email, List<string> Roles);
}
