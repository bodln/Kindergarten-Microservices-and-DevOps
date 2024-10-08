﻿using StudentsService.Data;
using StudentsService.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentsService.SyncDataServices.Grpc;

namespace StudentsService.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDbContext _context;
        private readonly IEventDataClient _eventDataClient;

        public StudentRepository(AppDbContext context,
            IEventDataClient eventDataClient)
        {
            _context = context;
            _eventDataClient = eventDataClient;
        }

        public async Task<Student> CreateStudent(StudentDTO studentDTO)
        {
            if (studentDTO.Grade < 1 || studentDTO.Grade > 5)
            {
                throw new ArgumentOutOfRangeException("Grade must be between 1 and 5.");
            }

            var newStudent = new Student
            {
                FirstName = studentDTO.FirstName,
                LastName = studentDTO.LastName,
                DateOfBirth = studentDTO.DateOfBirth,
                Age = studentDTO.Age,
                Grade = studentDTO.Grade
            };

            _context.Students.Add(newStudent);
            await _context.SaveChangesAsync();
            return newStudent;
        }

        public async Task<Student?> GetStudentById(int id)
        {
            return await _context.Students.FindAsync(id);
        }

        public async Task<List<Student>> GetAllStudents()
        {
            return await _context.Students.ToListAsync();
        }

        public async Task<Student?> UpdateStudent(int id, StudentDTO studentDTO)
        {
            if (studentDTO.Grade < 1 || studentDTO.Grade > 5)
            {
                throw new ArgumentOutOfRangeException("Grade must be between 1 and 5.");
            }

            var existingStudent = await _context.Students.FindAsync(id);
            if (existingStudent == null) return null;

            existingStudent.FirstName = studentDTO.FirstName;
            existingStudent.LastName = studentDTO.LastName;
            existingStudent.DateOfBirth = studentDTO.DateOfBirth;
            existingStudent.Age = studentDTO.Age;
            existingStudent.Grade = studentDTO.Grade;

            _context.Students.Update(existingStudent);
            await _context.SaveChangesAsync();
            return existingStudent;
        }

        public async Task<bool> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return false;

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> GradeStudent(int id, int grade, string nannyEmail)
        {
            if (grade < 1 || grade > 5)
            {
                throw new ArgumentOutOfRangeException("Grade must be between 1 and 5.");
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null) return false;

            student.Grade = grade;
            student.NannyEmail = nannyEmail;
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Participate(string token, int studentId, string eventId)
        {
            Student s = _context.Students.Find(studentId);

            if (s == null)
            {
                return false;
            }

            return await _eventDataClient.Participate(token, studentId, eventId, s.Grade);
        }
    }


    public interface IStudentRepository
    {
        Task<Student> CreateStudent(StudentDTO studentDTO);
        Task<Student?> GetStudentById(int id);
        Task<List<Student>> GetAllStudents();
        Task<Student?> UpdateStudent(int id, StudentDTO studentDTO);
        Task<bool> DeleteStudent(int id);
        Task<bool> GradeStudent(int id, int grade, string nannyEmail);
        Task<bool> Participate(string token, int studentId, string eventId);
    }


    public class StudentDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        public int Grade { get; set; }
    }
}