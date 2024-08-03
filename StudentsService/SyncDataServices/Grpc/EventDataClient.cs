using Grpc.Net.Client;
using StudentsService.Protos; // Ensure this namespace matches the one in the generated files

namespace StudentsService.SyncDataServices.Grpc
{
    public class EventDataClient : IEventDataClient
    {
        private readonly IConfiguration _configuration;

        public EventDataClient(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> Participate(string token, int studentId, string eventId, int grade)
        {
            Console.WriteLine("--> Calling gRPC Service: " + _configuration["GrpcPlatform"]);

            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            var channel = GrpcChannel.ForAddress(_configuration["GrpcPlatform"], new GrpcChannelOptions { HttpClient = new HttpClient(handler) });
            var client = new AssessStudentService.AssessStudentServiceClient(channel); 

            var request = new Student
            {
                StudentId = studentId,
                EventId = eventId,
                Token = token,
                Grade = grade
            };

            Console.WriteLine("--> Sending gRPC Request: " + request);

            var response = await client.AssessStudentAsync(request);

            Console.WriteLine("--> Recieved gRPC Response: " + response);

            return response.Approved; 
        }
    }
}
