using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentsService.Repositories;

[Route("api/[controller]")]
[ApiController]
public class StudentController : ControllerBase
{
    private readonly IStudentRepository _studentRepository;

    public StudentController(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    [HttpPost("Create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateStudent([FromBody] StudentDTO studentDTO)
    {
        if (studentDTO == null)
        {
            return BadRequest("Student data is null.");
        }

        try
        {
            var createdStudent = await _studentRepository.CreateStudent(studentDTO);
            return CreatedAtAction(nameof(GetStudentById), new { id = createdStudent.Id }, createdStudent);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Nanny")]
    public async Task<IActionResult> GetStudentById(int id)
    {
        var student = await _studentRepository.GetStudentById(id);
        if (student == null)
        {
            return NotFound("Student not found.");
        }

        return Ok(student);
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAllStudents()
    {
        var students = await _studentRepository.GetAllStudents();
        return Ok(students);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStudent(int id, [FromBody] StudentDTO studentDTO)
    {
        if (studentDTO == null)
        {
            return BadRequest("Student data is null.");
        }

        try
        {
            var updatedStudent = await _studentRepository.UpdateStudent(id, studentDTO);
            if (updatedStudent == null)
            {
                return NotFound("Student not found.");
            }

            return Ok(updatedStudent);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        var result = await _studentRepository.DeleteStudent(id);
        if (!result)
        {
            return NotFound("Student not found.");
        }

        return Ok("Student deleted successfully.");
    }

    [HttpPatch("{id:int}/Grade")]
    [Authorize(Roles = "Nanny")]
    public async Task<IActionResult> GradeStudent(int id, [FromQuery] int grade, string nannyEmail)
    {
        try
        {
            var result = await _studentRepository.GradeStudent(id, grade, nannyEmail);
            if (!result)
            {
                return NotFound("Student not found.");
            }

            return Ok("Student grade updated successfully.");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{studentId:int}/Participate/{eventId:string}")]
    [Authorize(Roles = "Nanny")]
    public async Task<IActionResult> GradeStudent(int studentId, string eventId)
    {
        string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var response = await _studentRepository.Participate(token, studentId, eventId);
        return response ? Ok("Student: " + studentId + ", participated in event: " + eventId) : NotFound("Student participation denied...");
    }
}
