using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uni_Connect.Models;
using Uni_Connect.Services;
using System.Security.Claims;

namespace Uni_Connect.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPointService _pointService;
        private readonly INotificationService _notificationService;

        public AdminController(ApplicationDbContext context, IPointService pointService , INotificationService notificationService)
        {
            _context = context;
            _pointService = pointService;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> AdminDashboard()
        {
            var userCount = await _context.Users.IgnoreQueryFilters().CountAsync();
            var postCount = await _context.Posts.IgnoreQueryFilters().CountAsync();
            var reportCount = await _context.Reports.CountAsync(r => !r.IsResolved);
            var BestAnswerRequestCount = await _context.BestAnswerRequests.CountAsync(r => !r.IsApproved);

            ViewBag.UserCount = userCount;
            ViewBag.PostCount = postCount;
            ViewBag.ReportCount = reportCount;
            ViewBag.BestAnswerRequestCount = BestAnswerRequestCount;

            return View();
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Users.IgnoreQueryFilters().OrderByDescending(u => u.CreatedAt).ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserDelete(int id)
        {
            var user = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user != null)
            {
                if (!user.IsDeleted)
                {
                    user.IsDeleted = true;
                    user.DeletedByAdmin = true;
                }
                else
                {
                    user.IsDeleted = false;
                    user.DeletedByAdmin = false;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ManageUsers");
        }

        public async Task<IActionResult> ManageReports()
        {
            var reports = await _context.Reports
                .Where(r => !r.IsDeleted)
                .Include(r => r.Reporter)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var postIds = reports
                .Where(r => r.TargetType == "Post")
                .Select(r => r.TargetID)
                .ToList();

            var answerIds = reports
                .Where(r => r.TargetType == "Answer")
                .Select(r => r.TargetID)
                .ToList();

            var postOwnerMap = await _context.Posts
                .Where(p => postIds.Contains(p.PostID))
                .Include(p => p.User)
                 .Select(p => new
                 {
                     p.PostID,
                     OwnerName = p.User.Name,
                     OwnerUserName = p.User.Username,
                     OwnerImageUrl = p.User.ProfileImageUrl
                 })
                 .ToDictionaryAsync(p => p.PostID);

            var answerInfoMap = await _context.Answers
                .Where(a => answerIds.Contains(a.AnswerID))
                .Include(a => a.User)
                .Select(a => new
                {
                    a.AnswerID,
                    a.PostID,
                    OwnerName = a.User.Name,
                    OnwerUserName = a.User.Username,
                    OwnerImageUrl = a.User.ProfileImageUrl
                })
                .ToDictionaryAsync(a => a.AnswerID);

            ViewBag.PostOwnerMap = postOwnerMap;
            ViewBag.AnswerInfoMap = answerInfoMap;

            return View(reports);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveReport(int id)
        {
            var report = await _context.Reports
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.ReportID == id);

            if (report != null)
            {
                report.IsResolved = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageReports");
        }

        public async Task<IActionResult> ManageContent(string tab = "posts")
        {
            var posts = await _context.Posts
                .IgnoreQueryFilters()
                .Include(p => p.User)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Take(100)
                .ToListAsync();

            var answers = await _context.Answers
                .IgnoreQueryFilters()
                .Include(a => a.User)
                .Include(a => a.Post)
                .OrderByDescending(a => a.CreatedAt)
                .Take(100)
                .ToListAsync();

            ViewBag.Posts = posts;
            ViewBag.Answers = answers;
            ViewBag.CurrentTab = tab;

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminDeletePost(int postId)
        {
            var post = await _context.Posts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.PostID == postId);

            if (post == null)
            {
                TempData["ErrorMessage"] = "Post not found.";
                return RedirectToAction("ManageContent");
            }

            post.IsDeleted = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Post deleted successfully.";
            return RedirectToAction("ManageContent", new { tab = "posts" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminDeleteAnswer(int answerId)
        {
            var answer = await _context.Answers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.AnswerID == answerId);

            if (answer == null)
            {
                TempData["ErrorMessage"] = "Answer not found.";
                return RedirectToAction("ManageContent", new { tab = "answers" });
            }

            answer.IsDeleted = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Answer deleted successfully.";
            return RedirectToAction("ManageContent", new { tab = "answers" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminEditPost(int postId, string title, string content)
        {
            var post = await _context.Posts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.PostID == postId);

            if (post == null)
            {
                TempData["ErrorMessage"] = "Post not found.";
                return RedirectToAction("ManageContent");
            }

            post.Title = title?.Trim() ?? post.Title;
            post.Content = content?.Trim() ?? post.Content;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Post updated successfully.";
            return RedirectToAction("ManageContent", new { tab = "posts" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminEditAnswer(int answerId, string content)
        {
            var answer = await _context.Answers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.AnswerID == answerId);

            if (answer == null)
            {
                TempData["ErrorMessage"] = "Answer not found.";
                return RedirectToAction("ManageContent", new { tab = "answers" });
            }

            answer.Content = content?.Trim() ?? answer.Content;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Answer updated successfully.";
            return RedirectToAction("ManageContent", new { tab = "answers" });
        }
        public async Task<IActionResult> DeletedPosts()
        {
            var posts = await _context.Posts
                .IgnoreQueryFilters()
                .Where(p => p.IsDeleted)
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Answers)
                    .ThenInclude(a => a.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }
        public async Task<IActionResult> BestAnswerRequests()
        {
            var requests = await _context.BestAnswerRequests
                .Where(r => !r.IsDeleted && !r.IsApproved && !r.IsRejected)
                .Include(r => r.Post)
                    .ThenInclude(p => p.User)
                .Include(r => r.Answer)
                    .ThenInclude(a => a.User)
                .Include(r => r.RequestedByUser)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBestAnswerRequest(int id)
        {
            var adminId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var request = await _context.BestAnswerRequests
                .Include(r => r.Answer)
                    .ThenInclude(a => a.Post)
                .FirstOrDefaultAsync(r =>
                    r.BestAnswerRequestID == id &&
                    !r.IsDeleted &&
                    !r.IsApproved &&
                    !r.IsRejected);

            if (request == null)
            {
                TempData["ErrorMessage"] = "Best Answer request not found.";
                return RedirectToAction("BestAnswerRequests");
            }

            var answer = request.Answer;

            if (answer == null || answer.IsDeleted)
            {
                TempData["ErrorMessage"] = "Answer no longer exists.";
                return RedirectToAction("BestAnswerRequests");
            }

            // Remove previous best answer for this post
            var previousBestAnswers = await _context.Answers
                .Where(a => a.PostID == request.PostID && a.IsAccepted)
                .ToListAsync();

            foreach (var oldAnswer in previousBestAnswers)
            {
                oldAnswer.IsAccepted = false;
            }

            answer.IsAccepted = true;

            request.IsApproved = true;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedByAdminID = adminId;

           
            await _context.SaveChangesAsync();

            // Award points only after admin approval
            await _pointService.AwardPoints(
                answer.UserID,
                15,
                "Answer marked as Best Answer",
                answer.Content.Length > 60 ? answer.Content[..60] + "…" : answer.Content,
                "⭐"
            );
            await _notificationService.CreateAsync(
                answer.UserID,
                "Your answer was marked as the best answer.",
                "BestAnswer",
                request.PostID,
                request.RequestedByUserID
            );

            TempData["SuccessMessage"] = "Best Answer approved and points awarded.";
            return RedirectToAction("BestAnswerRequests");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBestAnswerRequest(int id)
        {
            var adminId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var request = await _context.BestAnswerRequests
                .FirstOrDefaultAsync(r =>
                    r.BestAnswerRequestID == id &&
                    !r.IsDeleted &&
                    !r.IsApproved &&
                    !r.IsRejected);

            if (request == null)
            {
                TempData["ErrorMessage"] = "Best Answer request not found.";
                return RedirectToAction("BestAnswerRequests");
            }

            request.IsRejected = true;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedByAdminID = adminId;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Best Answer request rejected.";
            return RedirectToAction("BestAnswerRequests");
        }
    }
}
