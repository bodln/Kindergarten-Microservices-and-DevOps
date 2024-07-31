using Microsoft.IdentityModel.Tokens;
using StudentsService.DTOs;
using StudentsService.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

namespace StudentsService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;

        public EventProcessor(IServiceScopeFactory scopeFactory,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }

        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);

            switch (eventType)
            {
                case EventType.GradeStudent:
                    GradeStudent(message);
                    break;
                default:
                    break;
            }
        }

        private EventType DetermineEvent(string notificationMessage)
        {
            Console.WriteLine("--> Determining event...");

            var eventType = JsonSerializer.Deserialize<GenericEventDTO>(notificationMessage);
            Console.WriteLine("--> Event received: " + eventType.eventMessage + " \nFrom this message: " + notificationMessage);

            switch (eventType.eventMessage)
            {
                case "Grade_Student":
                    Console.WriteLine("--> Grade_Student event detected.");
                    return EventType.GradeStudent;
                default:
                    Console.WriteLine("--> Could not determine event.");
                    return EventType.Undetermined;
            }
        }

        private async void GradeStudent(string studentGradedMessage)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<IStudentRepository>();

                var studentGradedDTO = JsonSerializer.Deserialize<StudentGradedDTO>(studentGradedMessage);

                if (VerifyToken(studentGradedDTO.token))
                {
                    var response = await repo.GradeStudent(studentGradedDTO.studentId, studentGradedDTO.grade, studentGradedDTO.nannyEmail);

                    if (response)
                    {
                        Console.WriteLine("--> Student grading successful.");
                    }
                    else
                    {
                        Console.WriteLine("--> Student grading failed...");
                    }
                }
                else
                {
                    Console.WriteLine("--> Student grading failed due to invalid token...");
                }
            }
        }

        private bool VerifyToken(string token)
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

                Console.WriteLine("--> The passed in token successfully authenticated.");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("--> Error in validating token: " + ex.Message);
                return false;
            }
        }
    }

        enum EventType
        {
            GradeStudent,
            Undetermined
        }
    }
