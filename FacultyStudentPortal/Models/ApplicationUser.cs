using Microsoft.AspNetCore.Identity;

namespace FacultyStudentPortal.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }

        public string Role { get; set; } // "Faculty" or "Student"
    }

}
