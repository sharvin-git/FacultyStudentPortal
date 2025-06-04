namespace FacultyStudentPortal.ViewModels
{
    public class AssessSubmissionInfoViewModel
    {
        public int SubmissionId { get; set; }
        public int AssignmentId { get; set; }
        public string FullName { get; set; }
        public string FilePath { get; set; }
        public string Link { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Comment { get; set; }

        public List<AssessSubmissionCriteriaViewModel> CriteriaList { get; set; }
    }
}
