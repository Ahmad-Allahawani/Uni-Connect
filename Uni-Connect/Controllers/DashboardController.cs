using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Uni_Connect.Models;
using Uni_Connect.Services;
using Uni_Connect.ViewModels;

namespace Uni_Connect.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IPointService _pointService;
        private readonly IPostService _postService;
        private readonly INotificationService _notificationService;

        public DashboardController(ApplicationDbContext context, IWebHostEnvironment environment,
            IPointService pointService, IPostService postService, INotificationService notificationService)
        {
            _context = context;
            _environment = environment;
            _pointService = pointService;
            _postService = postService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");

            var posts = await _context.Posts
                .Where(p => !p.IsDeleted)
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Answers.Where(a => !a.IsDeleted))
                .OrderByDescending(p => p.CreatedAt)
                .Take(50)
                .ToListAsync();

            ViewBag.Posts = posts;
            return View(user);
        }

        public async Task<IActionResult> Profile()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login_Page", "Login");
            int userId = int.Parse(userIdStr);

            var user = await _context.Users
                .Include(u => u.Posts.Where(p => !p.IsDeleted))
                    .ThenInclude(p => p.Category)
                .Include(u => u.Posts.Where(p => !p.IsDeleted))
                    .ThenInclude(p => p.Answers.Where(a => !a.IsDeleted))
                .Include(u => u.Answers.Where(a => !a.IsDeleted))
                    .ThenInclude(a => a.Post)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null) return RedirectToAction("Login_Page", "Login");

            var sessionCount = await _context.PrivateSessions
                .CountAsync(s => (s.StudentID == userId || s.HelperID == userId) && !s.IsDeleted);

            ViewBag.SessionCount = sessionCount;
            return View(user);
        }

        public async Task<IActionResult> ViewProfile(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return RedirectToAction("Dashboard");

            var userProfile = await _context.Users
                .Include(u => u.Posts.Where(p => !p.IsDeleted))
                    .ThenInclude(p => p.Category)
                .Include(u => u.Posts.Where(p => !p.IsDeleted))
                    .ThenInclude(p => p.Answers.Where(a => !a.IsDeleted))
                .Include(u => u.Answers.Where(a => !a.IsDeleted))
                    .ThenInclude(a => a.Post)
                        .ThenInclude(p => p!.Category)
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && !u.IsDeleted);

            if (userProfile == null) return RedirectToAction("Dashboard");

            var sessionCount = await _context.PrivateSessions
                .CountAsync(s => (s.StudentID == userProfile.UserID || s.HelperID == userProfile.UserID) && !s.IsDeleted);
            ViewBag.SessionCount = sessionCount;

            return View("Profile", userProfile);
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");

            user.IsDeleted = true;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login_Page", "Login");
        }

        public async Task<IActionResult> Settings()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");

            var model = new SettingsViewModel
            {
                UserID = user.UserID,
                Name = user.Name,
                Email = user.Email,
                Faculty = user.Faculty,
                YearOfStudy = user.YearOfStudy,
                ProfileImageUrl = user.ProfileImageUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(SettingsViewModel model, IFormFile? profileImage)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");

            user.Name = model.Name?.Trim() ?? user.Name;
            user.Faculty = model.Faculty;
            user.YearOfStudy = model.YearOfStudy;

            // Handle profile image upload (file takes priority over URL)
            if (profileImage != null && profileImage.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                var ext = Path.GetExtension(profileImage.FileName).ToLowerInvariant();

                if (allowedExtensions.Contains(ext) &&
                    allowedMimeTypes.Contains(profileImage.ContentType.ToLowerInvariant()) &&
                    profileImage.Length <= 5 * 1024 * 1024)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + ext;
                    using var fs = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create);
                    await profileImage.CopyToAsync(fs);
                    user.ProfileImageUrl = $"/uploads/profiles/{uniqueFileName}";
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid file. Use JPG, PNG, GIF or WebP under 5 MB.";
                    return RedirectToAction("Settings");
                }
            }
            else
            {
                user.ProfileImageUrl = model.ProfileImageUrl;
            }

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                {
                    TempData["ErrorMessage"] = "Current password is required to change password.";
                    return View(model);
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    TempData["ErrorMessage"] = "New password and confirmation do not match.";
                    return View(model);
                }

                if (model.NewPassword.Length < 6)
                {
                    TempData["ErrorMessage"] = "New password must be at least 6 characters.";
                    return View(model);
                }

                bool isValid = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash);
                if (!isValid)
                {
                    TempData["ErrorMessage"] = "Current password is incorrect.";
                    return View(model);
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                TempData["SuccessMessage"] = "Password updated successfully.";
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Re-issue auth cookie so sidebar name & avatar reflect changes immediately
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, User.FindFirst(ClaimTypes.Role)?.Value ?? "User")
            };
            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                claims.Add(new Claim("ProfileImageUrl", user.ProfileImageUrl));

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

            if (TempData["SuccessMessage"] == null)
                TempData["SuccessMessage"] = "Profile settings updated successfully.";

            return RedirectToAction("Settings");
        }

        public async Task<IActionResult> Notifications()
        {
            var me = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var notifications = await _context.Notifications
                .Where(n => n.UserID == me)
                .Include(n => n.Actor)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            // Capture unread IDs before marking read so view still sees original state
            var unreadIds = notifications.Where(n => !n.IsRead).Select(n => n.NotificationID).ToList();

            if (unreadIds.Any())
            {
                await _context.Notifications
                    .Where(n => unreadIds.Contains(n.NotificationID))
                    .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
            }

            return View(notifications); // original IsRead values preserved in the list
        }

        public async Task<IActionResult> Leaderboard()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");

            string faculty = Request.Query["faculty"].ToString();
            string period = Request.Query["period"].ToString();
            int top = 100;
            int.TryParse(Request.Query["top"].ToString(), out top);
            if (top <= 0) top = 100;

            var query = _context.Users.Where(u => !u.IsDeleted);

            if (!string.IsNullOrEmpty(faculty))
                query = query.Where(u => u.Faculty == faculty);

            // Fix: filter by points earned in that period, not account creation date
            if (!string.IsNullOrEmpty(period) && period != "All Time")
            {
                DateTime since = period == "This Week"
                    ? DateTime.UtcNow.AddDays(-7)
                    : DateTime.UtcNow.AddMonths(-1);

                var activeUserIds = await _context.PointsTransactions
                    .Where(pt => pt.CreatedAt >= since && !pt.IsDeleted)
                    .Select(pt => pt.UserID)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(u => activeUserIds.Contains(u.UserID));
            }

            var leaderboardUsers = await query
                .OrderByDescending(u => u.Points)
                .Take(top)
                .ToListAsync();

            var faculties = await _context.Users
                .Where(u => !u.IsDeleted && !string.IsNullOrEmpty(u.Faculty))
                .Select(u => u.Faculty)
                .Distinct()
                .ToListAsync();

            ViewBag.Leaderboard = leaderboardUsers;
            ViewBag.Faculties = faculties;
            ViewBag.SelectedFaculty = faculty;
            ViewBag.SelectedPeriod = string.IsNullOrEmpty(period) ? "All Time" : period;
            ViewBag.Top = top;

            return View(user);
        }

        public async Task<IActionResult> Points()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");

            int level = Math.Min(user.Points / 500 + 1, 10);
            int pointsForCurrentLevel = (level - 1) * 500;
            int pointsForNextLevel = level * 500;
            int progressToNextLevel = Math.Max(0, user.Points - pointsForCurrentLevel);
            int pointsNeededForNext = Math.Max(0, pointsForNextLevel - user.Points);
            int progressPercentage = (int)((progressToNextLevel / (float)(pointsForNextLevel - pointsForCurrentLevel)) * 100);

            var userPosts = await _context.Posts.CountAsync(p => p.UserID == user.UserID && !p.IsDeleted);
            var userAnswers = await _context.Answers.CountAsync(a => a.UserID == user.UserID && !a.IsDeleted);

            var achievements = new List<Achievement>
            {
                new Achievement { Title = "First Steps", Description = "Score 100 points", Icon = "🎯", Unlocked = user.Points >= 100 },
                new Achievement { Title = "Helper", Description = "Give 5 answers", Icon = "🤝", Unlocked = userAnswers >= 5 },
                new Achievement { Title = "Questioner", Description = "Ask 3 questions", Icon = "❓", Unlocked = userPosts >= 3 }
            };

            var transactions = await _context.PointsTransactions
                .Where(pt => pt.UserID == user.UserID && !pt.IsDeleted)
                .OrderByDescending(pt => pt.CreatedAt)
                .Take(20)
                .Select(pt => new PointTransaction
                {
                    Title = pt.Title,
                    Icon = pt.Icon,
                    Time = pt.CreatedAt.ToString("MMM dd, HH:mm"),
                    Amount = pt.Amount,
                    Detail = pt.Detail
                })
                .ToListAsync();

            var model = new PointsViewModel
            {
                UserID = user.UserID,
                Name = user.Name,
                Faculty = user.Faculty,
                YearOfStudy = user.YearOfStudy,
                CurrentPoints = user.Points,
                CurrentLevel = level,
                NextLevelPoints = pointsNeededForNext,
                ProgressPercentage = progressPercentage,
                QuestionsAsked = userPosts,
                AnswersGiven = userAnswers,
                Achievements = achievements,
                Transactions = transactions
            };

            return View(model);
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

        public async Task<IActionResult> ChatPage()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");
            return View(user);
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

            if (post == null) return RedirectToAction("Dashboard");

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
        public async Task<IActionResult> PostAnswer(int postId, string content, IFormFile? ImageFile)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Answer content cannot be empty.";
                return RedirectToAction("SinglePost", new { id = postId });
            }

            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");

            var answer = await _postService.PostAnswer(postId, content, user.UserID, ImageFile);
            if (answer == null) return RedirectToAction("Dashboard");

            return RedirectToAction("SinglePost", new { id = postId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpvoteAnswer(int answerId)
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();

            var result = await _postService.UpvoteAnswer(answerId, user.UserID);
            if (result == null) return BadRequest(new { message = "Cannot upvote your own answer." });

            return Ok(new { success = true, voted = result.Value.voted, upvotes = result.Value.upvotes });
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

        public async Task<IActionResult> Search(string? q)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");

            var query = _context.Posts
                .Where(p => !p.IsDeleted)
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Answers.Where(a => !a.IsDeleted))
                .AsQueryable();

            // Use .Contains() directly — SQL Server default collation is case-insensitive
            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(p =>
                    p.Title.Contains(q) ||
                    p.Content.Contains(q) ||
                    (p.Category != null && p.Category.Name.Contains(q)));
            }

            var results = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            ViewBag.Query = q;
            ViewBag.Results = results;
            return View(user);
        }

        [HttpGet("/api/user/points")]
        public async Task<IActionResult> GetUserPoints()
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();
            int level = Math.Min(user.Points / 500 + 1, 10);
            int toNext = Math.Max(0, level * 500 - user.Points);
            return Ok(new { points = user.Points, level, toNext });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptAnswer(int answerId)
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();

            var answer = await _context.Answers
                .Include(a => a.Post)
                .FirstOrDefaultAsync(a => a.AnswerID == answerId && !a.IsDeleted);

            if (answer == null) return NotFound();
            if (answer.Post.UserID != user.UserID) return Forbid();

            var prev = await _context.Answers
                .Where(a => a.PostID == answer.PostID && a.IsAccepted)
                .ToListAsync();
            prev.ForEach(a => a.IsAccepted = false);

            answer.IsAccepted = true;
            await _context.SaveChangesAsync();

            await _pointService.AwardPoints(answer.UserID, 15,
                "Answer marked as Best Answer",
                answer.Content.Length > 60 ? answer.Content[..60] + "…" : answer.Content, "⭐");

            // Notify the answerer
            try
            {
                await _notificationService.CreateAsync(
                    answer.UserID,
                    "Your answer was marked as the Best Answer! +15 points",
                    "BestAnswer",
                    answer.PostID
                );
            }
            catch { /* notification failure doesn't block accept */ }

            return Ok(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickUpdatePhoto(IFormFile profileImage)
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();
            if (profileImage == null || profileImage.Length == 0)
                return Json(new { success = false, message = "No file provided." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var allowedMimeTypes  = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            var ext = Path.GetExtension(profileImage.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(ext) ||
                !allowedMimeTypes.Contains(profileImage.ContentType.ToLowerInvariant()) ||
                profileImage.Length > 5 * 1024 * 1024)
                return Json(new { success = false, message = "Invalid file. Use JPG, PNG, GIF or WebP under 5MB." });

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            var uniqueFileName = Guid.NewGuid().ToString() + ext;
            using var fs = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create);
            await profileImage.CopyToAsync(fs);
            user.ProfileImageUrl = $"/uploads/profiles/{uniqueFileName}";

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var existingClaims = User.Claims.Where(c => c.Type != "ProfileImageUrl").ToList();
            existingClaims.Add(new Claim("ProfileImageUrl", user.ProfileImageUrl));
            var identity = new ClaimsIdentity(existingClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return Json(new { success = true, imageUrl = user.ProfileImageUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();
            var deleted = await _postService.DeletePost(postId, user.UserID);
            if (!deleted) return Json(new { success = false, message = "Post not found or you don't own it." });
            TempData["SuccessMessage"] = "Your post has been deleted.";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnswer(int answerId, int postId)
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();
            await _postService.DeleteAnswer(answerId, user.UserID);
            TempData["SuccessMessage"] = "Your answer has been deleted.";
            return RedirectToAction("SinglePost", new { id = postId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                TargetID   = targetId,
                TargetType = targetType,
                Reason     = reason.Trim(),
                CreatedAt  = DateTime.UtcNow,
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
