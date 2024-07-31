namespace StudentsService.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        public int Grade { get; set; } = 0;
        public string NannyEmail { get; set; } = string.Empty;
    }
}
