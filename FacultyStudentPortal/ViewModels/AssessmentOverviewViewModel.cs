namespace FacultyStudentPortal.ViewModels
{
    public class AssessmentOverviewViewModel
    {
        public int AssignmentId { get; set; }
        public string Title { get; set; }
        public DateTime DueDate { get; set; }
        public int SubmissionCount { get; set; }
        public bool HasCriteria { get; set; }
    }
}
