
using Dapper;
using FacultyStudentPortal.Models;
using FacultyStudentPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;

namespace FacultyStudentPortal.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDbConnection _db;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public StudentController(UserManager<ApplicationUser> userManager, IDbConnection db, IWebHostEnvironment env, IConfiguration configuration)
        {
            _userManager = userManager;
            _db = db;
            _env = env;
            _configuration = configuration;
        }

        public IActionResult StudentDashboard()
        {
            return View();
        }

        // LIST ALL ASSIGNMENTS FOR THIS STUDENT 
        [HttpGet]
        public async Task<IActionResult> StudentViewAssignments()
        {
            var userId = _userManager.GetUserId(User);

            var assignments = await _db.QueryAsync<StudentAssignmentListViewModel>(
                "sp_GetAssignmentsForStudent",
                new { StudentId = userId },
                commandType: CommandType.StoredProcedure
            );

            // Sort by newest due date to see latest assignments first
            assignments = assignments.OrderByDescending(a => a.DueDate).ToList();

            return View(assignments);
        }

        [HttpGet]
        public async Task<IActionResult> SubmitAssignment(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@AssignmentId", id);

            var assignment = await _db.QueryFirstOrDefaultAsync<SubmitAssignmentViewModel>(
                "sp_GetAssignmentDetails",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (assignment == null)
                return NotFound();

            assignment.AssignmentId = id; 

            // fetch timestamp of submission - SubmittedAt for the current student
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(studentId))
            {
                var subParams = new DynamicParameters();
                subParams.Add("@AssignmentId", id);
                subParams.Add("@StudentId", studentId);

                var submissionTime = await _db.QueryFirstOrDefaultAsync<DateTime?>(
                    "sp_GetSubmissionTimestamp",
                    subParams,
                    commandType: CommandType.StoredProcedure
                );

                assignment.SubmittedAt = submissionTime;
            }

            return View(assignment);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAssignment(SubmitAssignmentViewModel model)
        {
            // ======= SERVER‐SIDE DEBUG LOGGING =======
            Console.WriteLine("===== SERVER LOG: Entered SubmitAssignment POST =====");
            Console.WriteLine($"DEBUG: Received AssignmentId       → {model.AssignmentId}");
            Console.WriteLine($"DEBUG: Received SubmissionType     → \"{model.SubmissionType}\"");
            Console.WriteLine($"DEBUG: Received LinkSubmission    → \"{model.LinkSubmission}\"");

            if (model.StudentUpload != null)
            {
                Console.WriteLine($"DEBUG: Received StudentUpload.FileName → {model.StudentUpload.FileName}");
                Console.WriteLine($"DEBUG: Received StudentUpload.Length   → {model.StudentUpload.Length}");
                Console.WriteLine($"DEBUG: Received StudentUpload.ContentType → {model.StudentUpload.ContentType}");
            }
            else
            {
                Console.WriteLine("DEBUG: model.StudentUpload is NULL (no file was bound).");
            }

            Console.WriteLine($"DEBUG: ModelState.IsValid → {ModelState.IsValid}");
            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"DEBUG: ModelState error on '{entry.Key}' → {error.ErrorMessage}");
                    }
                }
            }

            if (model.SubmissionType == "file" && model.StudentUpload == null)
            {
                ModelState.AddModelError("", "You selected File Upload, but no file was chosen.");
                Console.WriteLine("DEBUG: Added ModelState error → file chosen but StudentUpload is null.");
            }
            else if (model.SubmissionType == "link" && string.IsNullOrWhiteSpace(model.LinkSubmission))
            {
                ModelState.AddModelError("", "You selected Link Submission, but did not type a link.");
                Console.WriteLine("DEBUG: Added ModelState error → link chosen but LinkSubmission is blank.");
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("===== SERVER LOG: ModelState INVALID, re‐fetching assignment to re‐display =====");
                var assignment = await _db.QueryFirstOrDefaultAsync<SubmitAssignmentViewModel>(
                    "sp_GetAssignmentDetails",
                    new { AssignmentId = model.AssignmentId },
                    commandType: CommandType.StoredProcedure
                );
                if (assignment == null)
                {
                    Console.WriteLine("DEBUG: sp_GetAssignmentDetails returned NULL!");
                    ModelState.AddModelError("", "Assignment not found.");
                    return View(model);
                }

                Console.WriteLine($"DEBUG: Re‐fetched from DB → Title: {assignment.Title}, Description: {assignment.Description}, DueDate: {assignment.DueDate}, FacultyFile: {assignment.FacultyUploadedFilePath}");
                model.Title = assignment.Title;
                model.Description = assignment.Description;
                model.DueDate = assignment.DueDate;
                model.FacultyUploadedFilePath = assignment.FacultyUploadedFilePath;

                return View(model);
            }

            Console.WriteLine("===== SERVER LOG: ModelState VALID, proceeding with file/link save =====");


            // Handle file upload if the user chose "file"
            string savedFilePath = null;
            if (model.SubmissionType == "file" && model.StudentUpload != null && model.StudentUpload.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "student-submissions");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.StudentUpload.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.StudentUpload.CopyToAsync(stream);
                }

                savedFilePath = "/student-submissions/" + uniqueFileName;
            }

            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var parameters = new DynamicParameters();
            parameters.Add("@AssignmentId", model.AssignmentId);
            parameters.Add("@StudentId", studentId);
            parameters.Add("@FilePath", savedFilePath);                        
            parameters.Add("@Link", model.LinkSubmission);                      
            parameters.Add("@Comment", model.Comment);                          
            parameters.Add("@SubmittedAt", DateTime.Now);

            await _db.ExecuteAsync(
                "sp_CreateSubmission",
                parameters,
                commandType: CommandType.StoredProcedure);

            TempData["SuccessMessage"] = "Assignment submitted successfully!";
            return RedirectToAction("StudentViewAssignments");
        }

        public async Task<IActionResult> ViewGradedAssignments()
        {
            var user = await _userManager.GetUserAsync(User);
            var studentId = user.Id;

            var gradedAssignments = await _db.QueryAsync<ViewGradedAssignmentsViewModel>(
                "sp_GetGradedAssignments",
                new { StudentId = studentId },
                commandType: CommandType.StoredProcedure
            );

            return View(gradedAssignments);
        }

        public async Task<IActionResult> StudentViewCriteria(int submissionId)
        {
            var result = await _db.QueryAsync<StudentViewCriteriaViewModel>(
            "sp_StudentViewCriteria",
            new { SubmissionId = submissionId },
            commandType: CommandType.StoredProcedure);

            if (!result.Any())
                return NotFound("No assessment data found.");

            return View(result);
        }

        public async Task<IActionResult> LoadRubric(int submissionId)
        {
            var result = await _db.QueryAsync<StudentViewCriteriaViewModel>(
            "sp_StudentViewCriteria",
            new { SubmissionId = submissionId },
            commandType: CommandType.StoredProcedure);

            if (!result.Any())
                return Content("<div class='text-danger'>No rubric found.</div>");

            return PartialView("_RubricPartial", result);
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentLineChartData()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var data = await _db.QueryAsync<StudentPerformanceChartViewModel>(
                "sp_GetStudentPerformanceLineChart",
                new { StudentId = studentId },
                commandType: CommandType.StoredProcedure
            );

            return Json(data);
        }

        public IActionResult StudentPerformanceChart()
        {
            return View();
        }

    }
}
