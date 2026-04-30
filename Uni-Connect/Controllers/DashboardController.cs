using Microsoft.AspNetCore.Mvc;
using Uni_Connect.Models;
using Uni_Connect.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Uni_Connect.Services;

namespace Uni_Connect.Controllers
{

    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IPointService _pointService;
        private readonly IPostService _postService;

        public DashboardController(ApplicationDbContext context, IWebHostEnvironment environment, IPointService pointService, IPostService postService)
        {
            _context = context;
            _environment = environment;
            _pointService = pointService;
            _postService = postService;
        }
        public async Task<IActionResult> Dashboard()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login_Page", "Login");

            int userId = int.Parse(userIdStr);
            var user = await _context.Users
                .Include(u => u.Notifications)
                .Include(u => u.Posts)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null) return RedirectToAction("Login_Page", "Login");

            // Fetch all posts with related data
            var posts = await _context.Posts
                .Where(p => !p.IsDeleted)
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Answers)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.Posts = posts;
            return View(user);
        }
        public async Task<IActionResult> Profile()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");
            
            // Ensure navigation properties are loaded
            await _context.Entry(user)
                .Collection(u => u.Posts)
                .LoadAsync();
            await _context.Entry(user)
                .Collection(u => u.Answers)
                .LoadAsync();
            
            return View(user);
        }

        // View another user's public profile by username
        public async Task<IActionResult> ViewProfile(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return RedirectToAction("Dashboard");

            var userProfile = await _context.Users
                .Include(u => u.Posts)
                .Include(u => u.Answers)
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && !u.IsDeleted);

            if (userProfile == null) return RedirectToAction("Dashboard");

            return View("Profile", userProfile);
        }

        // Settings page (GET)
        public async Task<IActionResult> Settings()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");

            var model = new Uni_Connect.ViewModels.SettingsViewModel
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
        public async Task<IActionResult> Settings(Uni_Connect.ViewModels.SettingsViewModel model)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");

            // Profile Update
            user.Name = model.Name?.Trim() ?? user.Name;
            user.Faculty = model.Faculty;
            user.YearOfStudy = model.YearOfStudy;
            user.ProfileImageUrl = model.ProfileImageUrl;

            // Password Change Logic
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

                // Verify current password (using BCrypt)
                bool isValid = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash);
                if (!isValid)
                {
                    TempData["ErrorMessage"] = "Current password is incorrect.";
                    return View(model);
                }

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                TempData["SuccessMessage"] = "Password updated successfully.";
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            if (TempData["SuccessMessage"] == null)
            {
                TempData["SuccessMessage"] = "Profile settings updated successfully.";
            }

            return RedirectToAction("Settings");
        }
        public async Task<IActionResult> Notifications()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");
            return View(user);
        }
        public async Task<IActionResult> Leaderboard()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");
            
            // Default parameters (can be supplied via querystring)
            string faculty = Request.Query["faculty"].ToString();
            string period = Request.Query["period"].ToString();
            int top = 100;
            int.TryParse(Request.Query["top"].ToString(), out top);
            if (top <= 0) top = 100;

            var query = _context.Users.Where(u => !u.IsDeleted);

            if (!string.IsNullOrEmpty(faculty))
            {
                query = query.Where(u => u.Faculty == faculty);
            }

            if (!string.IsNullOrEmpty(period))
            {
                if (period == "This Month")
                {
                    var since = DateTime.UtcNow.AddMonths(-1);
                    query = query.Where(u => u.CreatedAt >= since);
                }
                else if (period == "This Week")
                {
                    var since = DateTime.UtcNow.AddDays(-7);
                    query = query.Where(u => u.CreatedAt >= since);
                }
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

            // Calculate level
            int level = Math.Min(user.Points / 500 + 1, 10);
            int pointsForCurrentLevel = (level - 1) * 500;
            int pointsForNextLevel = level * 500;
            int progressToNextLevel = Math.Max(0, user.Points - pointsForCurrentLevel);
            int pointsNeededForNext = Math.Max(0, pointsForNextLevel - user.Points);
            int progressPercentage = (int)((progressToNextLevel / (float)(pointsForNextLevel - pointsForCurrentLevel)) * 100);

            var userPosts = await _context.Posts.CountAsync(p => p.UserID == user.UserID && !p.IsDeleted);
            var userAnswers = await _context.Answers.CountAsync(a => a.UserID == user.UserID && !a.IsDeleted);

            var achievements = new List<ViewModels.Achievement>
            {
                new ViewModels.Achievement { Title = "First Steps", Description = "Score 100 points", Icon = "🎯", Unlocked = user.Points >= 100 },
                new ViewModels.Achievement { Title = "Helper", Description = "Give 5 answers", Icon = "🤝", Unlocked = userAnswers >= 5 },
                new ViewModels.Achievement { Title = "Questioner", Description = "Ask 3 questions", Icon = "❓", Unlocked = userPosts >= 3 }
            };

            // Get real transactions
            var transactions = await _context.PointsTransactions
                .Where(pt => pt.UserID == user.UserID && !pt.IsDeleted)
                .OrderByDescending(pt => pt.CreatedAt)
                .Take(20)
                .Select(pt => new ViewModels.PointTransaction
                {
                    Title = pt.Title,
                    Icon = pt.Icon,
                    Time = pt.CreatedAt.ToString("MMM dd, HH:mm"),
                    Amount = pt.Amount,
                    Detail = pt.Detail
                })
                .ToListAsync();

            var model = new ViewModels.PointsViewModel
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
                return View(new ViewModels.CreatePostViewModel());
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(ViewModels.CreatePostViewModel model)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            // Validate model
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

            // Get current user
            var user = await GetCurrentUser();
            if (user == null) 
            {
                if (isAjax) return Unauthorized();
                return RedirectToAction("Login_Page", "Login");
            }

            // Check if user has enough points
            if (user.Points < 10)
            {
                if (isAjax) return Json(new { success = false, message = "Insufficient points (10 required)." });
                ModelState.AddModelError("", "You need at least 10 points to post a question.");
                return View(model);
            }

            // Create Post via PostService
            var post = await _postService.CreatePost(model, user.UserID);
            if (post == null)
            {
                if (isAjax) return Json(new { success = false, message = "Failed to create post." });
                return View(model);
            }

            // Deduct points via PointService
            await _pointService.DeductPoints(user.UserID, 10, "Posted a Question", post.Title, "❓");

            if (isAjax) return Json(new { success = true, postId = post.PostID, pointsBalance = user.Points });
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> Sessions()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");
            
            var sessions = await _context.PrivateSessions
                .Where(s => s.StudentID == user.UserID || s.HelperID == user.UserID)
                .Include(s => s.Student)
                .Include(s => s.Helper)
                .Include(s => s.Messages)
                .ToListAsync();
            
            return View(sessions);
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

            // Increment view count (persist asynchronously)
            try
            {
                post.ViewsCount += 1;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Non-fatal if saving views fails
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

            // Post Answer via PostService
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

            var success = await _postService.UpvoteAnswer(answerId, user.UserID);
            if (!success) return NotFound();

            var answer = await _context.Answers.FindAsync(answerId);
            return Ok(new { success = true, upvotes = answer?.Upvotes ?? 0 });
        }

       private async Task<User?> GetCurrentUser()
       {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr)) return null;
        int userId = int.Parse(userIdStr);
        return await _context.Users
        .Include(u => u.Notifications)
        .FirstOrDefaultAsync(u => u.UserID == userId);
       }

        [HttpGet("/api/messages/{roomId}")]
        public async Task<IActionResult> GetMessages(int roomId)
        {
            var messages = await _context.Messages
                .Where(m => m.SessionID == roomId)
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    m.SenderID,
                    m.MessageText,
                    Time = m.SentAt.ToString("HH:mm")
                })
                 .ToListAsync();
            return Ok(messages);


        }
        private async Task<string?> SaveImage(IFormFile? file, string folder)
        {
            if (file == null || file.Length == 0) return null;

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{folder}/{uniqueFileName}";
        }
    }
}
