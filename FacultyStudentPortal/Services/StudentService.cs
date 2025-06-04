using FacultyStudentPortal.Data.Interfaces;
using FacultyStudentPortal.ViewModels;

namespace FacultyStudentPortal.Services
{
    public class StudentService
    {
        private readonly IStudentRepository _repo;

        public StudentService(IStudentRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<StudentListViewModel>> GetAllStudentsAsync()
        {
            return await _repo.GetAllStudentsAsync();
        }
    }
}
