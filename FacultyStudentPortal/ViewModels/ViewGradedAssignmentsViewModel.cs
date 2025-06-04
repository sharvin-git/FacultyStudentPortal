namespace FacultyStudentPortal.ViewModels
{
    public class ViewGradedAssignmentsViewModel
    {
        public int AssignmentId { get; set; }
        public int SubmissionId { get; set; }
        public string Title { get; set; }
        public int TotalMark { get; set; }
        public int TotalScore { get; set; }
    }
}
