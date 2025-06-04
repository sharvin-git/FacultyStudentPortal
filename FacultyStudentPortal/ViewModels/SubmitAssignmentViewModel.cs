
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FacultyStudentPortal.ViewModels
{
    public class SubmitAssignmentViewModel
    {

        [Required]
        public int AssignmentId { get; set; }

        [Display(Name = "Upload File")]
        public IFormFile? StudentUpload { get; set; }

        public string? LinkSubmission { get; set; } 

        [Display(Name = "Comment (Optional)")]
        public string? Comment { get; set; }

        public string SubmissionType { get; set; } // "file" or "link"


        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public string? FacultyUploadedFilePath { get; set; }
        // Holds the timestamp of the student’s submission (null if not yet submitted)
        public DateTime? SubmittedAt { get; set; }
    }
}
