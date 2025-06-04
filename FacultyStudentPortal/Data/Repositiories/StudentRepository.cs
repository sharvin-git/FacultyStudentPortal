using FacultyStudentPortal.Data.Interfaces;
using FacultyStudentPortal.ViewModels;
using System.Data;
using Dapper;

namespace FacultyStudentPortal.Data.Repositiories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly IDbConnection _db;

        public StudentRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<StudentListViewModel>> GetAllStudentsAsync()
        {
            return await _db.QueryAsync<StudentListViewModel>(
                "sp_GetAllStudents",
                commandType: CommandType.StoredProcedure
            );
        }
    }

}
