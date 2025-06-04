namespace FacultyStudentPortal.ViewModels
{
    public class CreateAssessmentViewModel
    {
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; }
        public List<AssessmentCriterionViewModel> Criteria { get; set; } = new();
        public string Remarks { get; set; }  
    }
}
