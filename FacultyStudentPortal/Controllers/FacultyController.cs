using Dapper;
using FacultyStudentPortal.Models;
using FacultyStudentPortal.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenAI_API;
using System.Data;
using System.IO;
using System.Security.Claims;
using System.Text;

namespace FacultyStudentPortal.Controllers
{
    public class FacultyController : Controller
    {
        public IActionResult FacultyDashboard()
        {
            return View();
        }

        private readonly IDbConnection _db;
        private readonly IWebHostEnvironment _env;
        private readonly HFService _hfService;

        public FacultyController(IDbConnection db, IWebHostEnvironment env, HFService hfService)
        {
            _db = db;
            _env = env;
            _hfService = hfService;
        }

        public async Task<IActionResult> StudentListFromSP()
        {
            var students = await _db.QueryAsync<StudentListViewModel>(
                "sp_GetAllStudents",
                commandType: CommandType.StoredProcedure
            );

            return View("StudentList", students);
        }

        // Show the Create Assignment Form
        [HttpGet]
        public IActionResult CreateAssignment()
        {
            return View();
        }

        // POST - Process the submitted form
        [HttpPost]
        public async Task<IActionResult> CreateAssignment(CreateAssignmentViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string filePathInDb = null;

            // Handle optional file upload (if file is submitted instead of link)
            if (model.UploadFile != null && model.UploadFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.UploadFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.UploadFile.CopyToAsync(stream);
                }

                filePathInDb = "/uploads/" + uniqueFileName; // Relative path to store in DB
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var parameters = new DynamicParameters();
            parameters.Add("@Title", model.Title);
            parameters.Add("@Description", model.Description);
            parameters.Add("@DueDate", model.DueDate);
            parameters.Add("@CreatedBy", userId);
            parameters.Add("@FilePath", filePathInDb); // NULL will be passed if no file is uploaded


            await _db.ExecuteAsync("sp_CreateAssignment", parameters, commandType: CommandType.StoredProcedure);

            TempData["SuccessMessage"] = "Assignment created successfully!";
            return RedirectToAction("ViewAssignments");
        }

        public async Task<IActionResult> ViewAssignments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // logged-in user's ID

            var parameters = new { FacultyId = userId };

            var assignments = await _db.QueryAsync<AssignmentListViewModel>(
                "sp_GetAssignmentsByFaculty",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return View(assignments);
        }

        [HttpGet]
        public async Task<IActionResult> EditAssignment(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@AssignmentId", id);

            var assignment = await _db.QueryFirstOrDefaultAsync<Assignment>(
                "sp_GetAssignmentById",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (assignment == null)
                return NotFound();

            var model = new EditAssignmentViewModel
            {
                AssignmentId = assignment.AssignmentId,
                Title = assignment.Title,
                Description = assignment.Description,
                DueDate = assignment.DueDate,
                ExistingFilePath = assignment.FilePath
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> EditAssignment(EditAssignmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string filePathInDb = model.ExistingFilePath;

            if (model.UploadFile != null && model.UploadFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");

                // Ensure folder exists
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid() + "_" + model.UploadFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.UploadFile.CopyToAsync(stream);

                filePathInDb = "/uploads/" + uniqueFileName;
            }

            var parameters = new DynamicParameters();
            parameters.Add("@AssignmentId", model.AssignmentId);
            parameters.Add("@Title", model.Title);
            parameters.Add("@Description", model.Description);
            parameters.Add("@DueDate", model.DueDate);
            parameters.Add("@FilePath", filePathInDb); // Can be null or a new file

            await _db.ExecuteAsync("sp_UpdateAssignment", parameters, commandType: CommandType.StoredProcedure);

            TempData["SuccessMessage"] = "Assignment updated!";
            return RedirectToAction("ViewAssignments");
        }


        [HttpGet]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            await _db.ExecuteAsync("sp_DeleteAssignment", new { AssignmentId = id }, commandType: CommandType.StoredProcedure);
            TempData["SuccessMessage"] = "Assignment deleted.";
            return RedirectToAction("ViewAssignments");
        }


        // Assessments Overview page
        [HttpGet]
        public async Task<IActionResult> AssessmentOverview()
        {
            var list = await _db.QueryAsync<AssessmentOverviewViewModel>(
                "sp_GetAssessmentOverview",
                commandType: CommandType.StoredProcedure);
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> CreateAssessment(int assignmentId)
        {
            var assignment = await _db.QueryFirstOrDefaultAsync<Assignment>(
                "sp_GetAssignmentById",
                new { AssignmentId = assignmentId },
                commandType: CommandType.StoredProcedure);

            if (assignment == null)
                return NotFound();

            var vm = new CreateAssessmentViewModel
            {
                AssignmentId = assignment.AssignmentId,
                AssignmentTitle = assignment.Title,
                Criteria = new List<AssessmentCriterionViewModel> { new() }
            };
            return View(vm);
        }


        // Handle form Post
        [HttpPost]
        public async Task<IActionResult> CreateAssessment(CreateAssessmentViewModel model)
        {
            // basic server validation
            if (model.Criteria == null || !model.Criteria.Any() ||
                model.Criteria.Any(c => string.IsNullOrWhiteSpace(c.CriterionName) || c.MaxScore < 1))
            {
                ModelState.AddModelError("", "Fill in every criterion name and a valid score.");
                return View(model);
            }

            // save each criterion
            foreach (var c in model.Criteria)
            {
                await _db.ExecuteAsync(
                    "sp_AddAssessmentCriterion",
                    new { AssignmentId = model.AssignmentId, CriterionName = c.CriterionName, MaxScore = c.MaxScore },
                    commandType: CommandType.StoredProcedure);
            }

            // redirect back to overview page
            return RedirectToAction(nameof(AssessmentOverview));
        }

        [HttpGet]
        public async Task<IActionResult> EditAssessment(int assignmentId)
        {
            var assignment = await _db.QueryFirstOrDefaultAsync<Assignment>(
                "sp_GetAssignmentById",
                new { AssignmentId = assignmentId },
                commandType: CommandType.StoredProcedure);

            if (assignment == null)
                return NotFound();

            var criteriaList = (await _db.QueryAsync<(string CriterionName, int MaxScore)>(
                "sp_GetCriteriaByAssignmentId",
                new { AssignmentId = assignmentId },
                commandType: CommandType.StoredProcedure)).ToList();

            var vm = new EditAssessmentViewModel
            {
                AssignmentId = assignment.AssignmentId,
                AssignmentTitle = assignment.Title,
                Criteria = criteriaList.Select(c => new AssessmentCriterionViewModel
                {
                    CriterionName = c.CriterionName,
                    MaxScore = c.MaxScore
                }).ToList()
            };

            if (!vm.Criteria.Any())
                vm.Criteria.Add(new AssessmentCriterionViewModel());

            return View("EditAssessment", vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditAssessment(EditAssessmentViewModel model)
        {
            // Validate inputs
            if (model.Criteria == null || !model.Criteria.Any() ||
                model.Criteria.Any(c => string.IsNullOrWhiteSpace(c.CriterionName) || c.MaxScore < 1))
            {
                ModelState.AddModelError("", "Fill in every criterion name and a valid score.");
                return View("EditAssessment", model);
            }

            // Delete old criteria
            await _db.ExecuteAsync(
                "sp_DeleteAllCriteriaByAssignmentId",
                new { AssignmentId = model.AssignmentId },
                commandType: CommandType.StoredProcedure);

            // Reinsert new ones
            foreach (var c in model.Criteria)
            {
                await _db.ExecuteAsync(
                    "sp_AddAssessmentCriterion",
                    new { AssignmentId = model.AssignmentId, CriterionName = c.CriterionName, MaxScore = c.MaxScore },
                    commandType: CommandType.StoredProcedure);
            }

            TempData["SuccessMessage"] = "Assessment updated successfully!";
            return RedirectToAction(nameof(AssessmentOverview));
        }

        public async Task<IActionResult> ViewSubmissions(int id) // id = AssignmentId
        {
            var submissions = await _db.QueryAsync<ViewSubmissionsViewModel>(
                "sp_GetSubmissionsByAssignment",
                new { AssignmentId = id },
                commandType: CommandType.StoredProcedure
            );

            return View("ViewSubmissions", submissions);
        }

        public async Task<IActionResult> AssessSubmission(int submissionId)
        {
            using var conn = _db;

            var result = await conn.QueryMultipleAsync("sp_GetAssessmentDetailsBySubmission",
                new { SubmissionId = submissionId }, commandType: CommandType.StoredProcedure);

            var studentInfo = await result.ReadFirstAsync<AssessSubmissionInfoViewModel>();
            var criteriaList = (await result.ReadAsync<AssessSubmissionCriteriaViewModel>()).ToList();

            // Attach criteria list to the student info model
            studentInfo.CriteriaList = criteriaList;

            return View(studentInfo);
        }

        [HttpPost]
        public async Task<IActionResult> AssessSubmission(AssessSubmissionInfoViewModel model)
        {
            foreach (var input in model.CriteriaList)
            {
                var parameters = new
                {
                    SubmissionId = model.SubmissionId,
                    CriteriaId = input.CriteriaId,
                    Score = input.Score,
                    Remark = input.Remark
                };

                await _db.ExecuteAsync("sp_SaveAssessment", parameters, commandType: CommandType.StoredProcedure);
            }

            return RedirectToAction("ViewSubmissions", new { id = model.AssignmentId });
        }

        [HttpGet]
        public async Task<IActionResult> GetLineChartData()
        {
            var data = await _db.QueryAsync<AverageScoreViewModel>(
                "sp_GetAverageScores",
                commandType: CommandType.StoredProcedure
            );

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetPieChartData(int assignmentId)
        {
            var data = await _db.QueryAsync<GradeDistributionViewModel>(
                "sp_GetGradeDistribution",
                new { AssignmentId = assignmentId },
                commandType: CommandType.StoredProcedure
            );

            return Json(data);
        }

        public async Task<IActionResult> PerformanceCharts()
        {
            var facultyId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var assignments = await _db.QueryAsync<AssignmentListViewModel>(
                "sp_GetAssignmentsByFaculty",
                new { FacultyId = facultyId }
            );

            ViewBag.Assignments = assignments;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAIInsights(int submissionId)
        {
            try
            {
                Console.WriteLine($"[GetAIInsights] Called with submissionId: {submissionId}");

                var submission = await _db.QueryFirstOrDefaultAsync<AIFeedbackViewModel>(
                    "sp_GetSubmissionDetailsForAI",
                    new { SubmissionId = submissionId },
                    commandType: CommandType.StoredProcedure
                );

                if (submission == null || string.IsNullOrEmpty(submission.FilePath))
                {
                    return BadRequest("Invalid submission or file missing.");
                }

                string uploadsFolder = Path.Combine(_env.WebRootPath, "student-submissions");
                string fileName = Path.GetFileName(submission.FilePath);
                string filePath = Path.Combine(uploadsFolder, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("File not found.");
                }

                var content = await System.IO.File.ReadAllTextAsync(filePath);

                // Guide the model to get constructive feedback
                var prompt = new StringBuilder();
                prompt.AppendLine("Instruction: Provide a brief academic feedback paragraph including one strength and one weakness.");
                prompt.AppendLine("Do NOT copy or summarize the text.");
                prompt.AppendLine("Use formal tone. Refer to the student in third person.");
                prompt.AppendLine("Respond ONLY with the feedback. Do not echo the input.");
                prompt.AppendLine();
                prompt.AppendLine("### START OF STUDENT SUBMISSION ###");
                prompt.AppendLine(content);
                prompt.AppendLine("### END OF STUDENT SUBMISSION ###");
                prompt.AppendLine();
                prompt.AppendLine("### FEEDBACK:");


                var response = await _hfService.GenerateFeedbackFromContentAsync(prompt.ToString());

                return Json(new { success = true, feedback = response.Trim() });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAIInsights] Exception: {ex}");
                return StatusCode(500, $"AI feedback generation failed: {ex.Message}");
            }
            // testing CI trigger

        }

    }
}
