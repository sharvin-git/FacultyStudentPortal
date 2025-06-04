namespace FacultyStudentPortal.ViewModels
{
    public class AssignmentListViewModel
    {
        public int AssignmentId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string CreatedByName { get; set; }
        public string FilePath { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
