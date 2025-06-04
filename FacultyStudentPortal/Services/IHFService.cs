namespace FacultyStudentPortal.Services
{
    public interface IHFService
    {
        Task<string> GenerateFeedbackFromContentAsync(string prompt);
    }
}
