namespace FacultyStudentPortal.ViewModels
{
    public class AssessSubmissionCriteriaViewModel
    {
        public int CriteriaId { get; set; }
        public string CriterionName { get; set; }
        public int MaxScore { get; set; }

        // Inputs from faculty
        public int Score { get; set; }
        public string Remark { get; set; }
    }
}
