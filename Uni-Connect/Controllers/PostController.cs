using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uni_Connect.Models;
using Uni_Connect.Services;
using Uni_Connect.ViewModels;

namespace Uni_Connect.Controllers
{
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IPointService _pointService;
        private readonly IPostService _postService;
        private readonly INotificationService _notificationService;

        public PostController(ApplicationDbContext context, IWebHostEnvironment environment,
            IPointService pointService, IPostService postService, INotificationService notificationService)
        {
            _context = context;
            _environment = environment;
            _pointService = pointService;
            _postService = postService;
            _notificationService = notificationService;
        }
        public async Task<IActionResult> CreatePost()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");
            return View(new CreatePostViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(CreatePostViewModel model)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (!ModelState.IsValid)
            {
                if (isAjax)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, message = "Validation failed: " + string.Join(", ", errors) });
                }
                return View(model);
            }

            var user = await GetCurrentUser();
            if (user == null)
            {
                if (isAjax) return Unauthorized();
                return RedirectToAction("Login_Page", "Login");
            }

            // Deduct points FIRST — prevent free posts if post creation fails
            bool deducted = await _pointService.DeductPoints(user.UserID, 10, "Posted a Question", model.Title, "❓");
            if (!deducted)
            {
                if (isAjax) return Json(new { success = false, message = "Insufficient points. You need 10 points to post." });
                ModelState.AddModelError("", "You need at least 10 points to post a question.");
                return View(model);
            }

            var post = await _postService.CreatePost(model, user.UserID);
            if (post == null)
            {
                // Refund if post creation failed
                await _pointService.AwardPoints(user.UserID, 10, "Post creation refund", null, "↩️");
                if (isAjax) return Json(new { success = false, message = "Failed to create post." });
                return View(model);
            }

            if (isAjax) return Json(new { success = true, postId = post.PostID, pointsBalance = user.Points - 10 });
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int postId, string title, string content, IFormFile? ImageFile, bool removeImage = false)
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();

            var post = await _context.Posts.FirstOrDefaultAsync(p =>
                p.PostID == postId &&
                p.UserID == user.UserID &&
                !p.IsDeleted);

            if (post == null) return Forbid();

            post.Title = title?.Trim() ?? post.Title;
            post.Content = content?.Trim() ?? post.Content;

            if (removeImage)
            {
                post.ImageUrl = null;
            }

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(ext) || ImageFile.Length > 5 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "Invalid image. Use JPG, PNG, GIF or WebP under 5MB.";
                    return RedirectToAction("SinglePost", new { id = postId });
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "posts");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid() + ext;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var fs = new FileStream(filePath, FileMode.Create);
                await ImageFile.CopyToAsync(fs);

                post.ImageUrl = $"/uploads/posts/{uniqueFileName}";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("SinglePost", new { id = postId });
        }


        public async Task<IActionResult> SinglePost(int id)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");

            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Answers)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(p => p.PostID == id);

            if (post == null) return RedirectToAction("Dashboard" ,"Dashboard");

            // Count unique views only (one per user, excluding post owner)
            if (post.UserID != user.UserID)
            {
                try
                {
                    bool alreadyViewed = await _context.PostViews
                        .AnyAsync(pv => pv.PostID == post.PostID && pv.UserID == user.UserID);
                    if (!alreadyViewed)
                    {
                        _context.PostViews.Add(new PostView { UserID = user.UserID, PostID = post.PostID });
                        post.ViewsCount += 1;
                        _context.Posts.Update(post);
                        await _context.SaveChangesAsync();
                    }
                }
                catch { /* Non-fatal */ }
            }

            return View(post);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpvotePost(int postId)
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();

            var result = await _postService.UpvotePost(postId, user.UserID);
            if (result == null) return BadRequest(new { message = "Cannot upvote your own post." });

            return Ok(new { success = true, voted = result.Value.voted, upvotes = result.Value.upvotes });
        }



        [HttpGet("/api/user/votes")]
        public async Task<IActionResult> GetUserVotes()
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();

            var postIds = await _context.PostVotes
                .Where(pv => pv.UserID == user.UserID)
                .Select(pv => pv.PostID)
                .ToListAsync();

            var answerIds = await _context.AnswerVotes
                .Where(av => av.UserID == user.UserID)
                .Select(av => av.AnswerID)
                .ToListAsync();

            return Ok(new { postIds, answerIds });
        }


        public async Task<IActionResult> DeletePost(int postId)
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();
            var deleted = await _postService.DeletePost(postId, user.UserID);
            if (!deleted) return Json(new { success = false, message = "Post not found or you don't own it." });
            TempData["SuccessMessage"] = "Your post has been deleted.";
            return RedirectToAction("Dashboard", "Dashboard");
        }


        public async Task<IActionResult> ReportContent(int targetId, string targetType, string reason)
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();

            var validTypes = new[] { "Post", "Answer" };
            if (!validTypes.Contains(targetType))
                return Json(new { success = false, message = "Invalid report type." });

            if (string.IsNullOrWhiteSpace(reason) || reason.Length > 500)
                return Json(new { success = false, message = "Please provide a reason (max 500 characters)." });

            bool alreadyReported = await _context.Reports.AnyAsync(r =>
                r.ReporterID == user.UserID && r.TargetID == targetId && r.TargetType == targetType);
            if (alreadyReported)
                return Json(new { success = false, message = "You have already reported this content." });

            _context.Reports.Add(new Report
            {
                ReporterID = user.UserID,
                TargetID = targetId,
                TargetType = targetType,
                Reason = reason.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsResolved = false
            });
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        private async Task<User?> GetCurrentUser()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return null;
            int userId = int.Parse(userIdStr);
            return await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
        }
       
    }
}
