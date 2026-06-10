using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniConnect.Web.Data;
using UniConnect.Web.Models;

namespace UniConnect.Web.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly UniConnectDbContext _context;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(ILogger<ReportsController> logger, UniConnectDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: Reports
        public async Task<IActionResult> Index()
        {
            var userId = User.GetUserId();
            
            var reports = await _context.Reports
                .Include(r => r.ReportedUser)
                .Where(r => r.ReporterId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reports);
        }

        // GET: Reports/Create
        public async Task<IActionResult> Create(string? type, int? targetId, string? reason)
        {
            var model = new ReportCreateViewModel
            {
                TargetType = type,
                TargetId = targetId,
                Reason = reason
            };

            ViewBag.Reasons = new List<string>
            {
                "Inappropriate content",
                "Spam",
                "Harassment",
                "Academic dishonesty",
                "Misinformation",
                "Other"
            };

            return View(model);
        }

        // POST: Reports/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TargetType,TargetId,Reason,Description")] ReportCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var reporterId = User.GetUserId();
                string? reportedUserId = null;

                // Find the reported user based on target type
                switch (model.TargetType)
                {
                    case "Post":
                        var post = await _context.Posts.FindAsync(model.TargetId);
                        reportedUserId = post?.UserId;
                        break;
                    case "Comment":
                        var comment = await _context.Comments.FindAsync(model.TargetId);
                        reportedUserId = comment?.UserId;
                        break;
                    case "User":
                        reportedUserId = model.TargetId.ToString();
                        break;
                    case "Request":
                        var request = await _context.AcademicRequests.FindAsync(model.TargetId);
                        reportedUserId = request?.UserId;
                        break;
                    case "Session":
                        var session = await _context.PrivateSessions.FindAsync(model.TargetId);
                        if (session != null)
                        {
                            // Report both participants
                            reportedUserId = session.StudentId == reporterId ? session.TutorId : session.StudentId;
                        }
                        break;
                }

                // Don't report yourself
                if (reportedUserId == reporterId)
                {
                    ModelState.AddModelError(string.Empty, "You cannot report yourself.");
                    ViewBag.Reasons = GetReasons();
                    return View(model);
                }

                var report = new Report
                {
                    ReporterId = reporterId,
                    ReportedUserId = reportedUserId,
                    Type = model.TargetType ?? "Unknown",
                    Reason = model.Reason ?? "Not specified",
                    Description = model.Description,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Report submitted successfully. Thank you for helping keep our community safe.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Reasons = GetReasons();
            return View(model);
        }

        // POST: Reports/QuickReport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickReport(string targetType, int targetId, string reason)
        {
            var reporterId = User.GetUserId();
            string? reportedUserId = null;

            // Find the reported user based on target type
            switch (targetType)
            {
                case "Post":
                    var post = await _context.Posts.FindAsync(targetId);
                    reportedUserId = post?.UserId;
                    break;
                case "Comment":
                    var comment = await _context.Comments.FindAsync(targetId);
                    reportedUserId = comment?.UserId;
                    break;
                case "User":
                    reportedUserId = targetId.ToString();
                    break;
                case "Request":
                    var request = await _context.AcademicRequests.FindAsync(targetId);
                    reportedUserId = request?.UserId;
                    break;
            }

            // Don't report yourself
            if (reportedUserId == reporterId)
            {
                return Json(new { success = false, message = "You cannot report yourself." });
            }

            var report = new Report
            {
                ReporterId = reporterId,
                ReportedUserId = reportedUserId,
                Type = targetType,
                Reason = reason,
                Description = $"Quick report from post/comment view",
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Report submitted successfully." });
        }

        private List<string> GetReasons()
        {
            return new List<string>
            {
                "Inappropriate content",
                "Spam",
                "Harassment",
                "Academic dishonesty",
                "Misinformation",
                "Other"
            };
        }
    }

    public class ReportCreateViewModel
    {
        public string? TargetType { get; set; }
        public int? TargetId { get; set; }
        public string? Reason { get; set; }
        public string? Description { get; set; }
    }
}
