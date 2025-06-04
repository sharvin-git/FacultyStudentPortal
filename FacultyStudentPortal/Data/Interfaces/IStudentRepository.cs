using FacultyStudentPortal.ViewModels;

namespace FacultyStudentPortal.Data.Interfaces
{
    public interface IStudentRepository
    {
        Task<IEnumerable<StudentListViewModel>> GetAllStudentsAsync();
    }
}
