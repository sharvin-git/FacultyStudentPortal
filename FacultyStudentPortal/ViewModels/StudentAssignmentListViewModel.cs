namespace FacultyStudentPortal.ViewModels
{
    public class StudentAssignmentListViewModel
    {
        // This will be used as the route/id when generating the "View / Submit" link
        public int AssignmentId { get; set; }

        // Shown in the table under “Title”
        public string Title { get; set; }

        // Shown in the table under “Due Date”
        public DateTime DueDate { get; set; }

        // The faculty (username) who created this assignment
        public string CreatedBy { get; set; }

        // “Submitted” / “Not Submitted” / “Closed”
        public string Status { get; set; }

    }

}
