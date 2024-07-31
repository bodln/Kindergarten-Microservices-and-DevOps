using NanniesService.DTOs;

namespace NanniesService.AsyncDataServices
{
    public interface IMessageBusClient
    {
        void GradeStudent(GradeStudentDTO getAllStudentsDTO);
    }
}
