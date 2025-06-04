namespace FacultyStudentPortal.ViewModels
{
    public class ViewSubmissionsViewModel
    {
        public int SubmissionId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime SubmittedAt { get; set; }
        public int? TotalMark { get; set; } // Can be null before assessment
        public int TotalScore { get; set; }
    }
}
