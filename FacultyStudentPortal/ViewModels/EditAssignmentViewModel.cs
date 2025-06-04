using System;
using System.ComponentModel.DataAnnotations;

namespace FacultyStudentPortal.ViewModels
{
    public class EditAssignmentViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [Display(Name = "Upload File")]
        public IFormFile? UploadFile { get; set; }  // for file upload input

        public int AssignmentId { get; set; } // For editing
        public string? ExistingFilePath { get; set; } // For keeping old file if no new upload

    }
}

