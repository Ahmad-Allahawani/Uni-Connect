using Microsoft.AspNetCore.Mvc;
using Uni_Connect.Models;
using Uni_Connect.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Uni_Connect.Controllers
{

    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
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
            
            // Fetch all users ranked by points
            var leaderboardUsers = await _context.Users
                .Where(u => !u.IsDeleted)
                .OrderByDescending(u => u.Points)
                .Take(100)
                .ToListAsync();

            ViewBag.Leaderboard = leaderboardUsers;
            return View(user);
        }

        public async Task<IActionResult> Points()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");

            // Calculate level (every 500 points = 1 level, max 10)
            int level = Math.Min(user.Points / 500 + 1, 10);
            int pointsForCurrentLevel = (level - 1) * 500;
            int pointsForNextLevel = level * 500;
            int progressToNextLevel = Math.Max(0, user.Points - pointsForCurrentLevel);
            int pointsNeededForNext = Math.Max(0, pointsForNextLevel - user.Points);
            int progressPercentage = (int)((progressToNextLevel / (float)(pointsForNextLevel - pointsForCurrentLevel)) * 100);

            // Get user's posts and answers count
            var userPosts = await _context.Posts
                .Where(p => p.UserID == user.UserID && !p.IsDeleted)
                .CountAsync();

            var userAnswers = await _context.Answers
                .Where(a => a.UserID == user.UserID && !a.IsDeleted)
                .CountAsync();

            // Build achievements list
            var achievements = new List<ViewModels.Achievement>
            {
                new ViewModels.Achievement
                {
                    Title = "First Steps",
                    Description = "Score 100 points",
                    Icon = "🎯",
                    Unlocked = user.Points >= 100,
                    UnlockedDate = user.Points >= 100 ? DateTime.Now : null
                },
                new ViewModels.Achievement
                {
                    Title = "Helper",
                    Description = "Give 5 answers",
                    Icon = "🤝",
                    Unlocked = userAnswers >= 5,
                    UnlockedDate = userAnswers >= 5 ? DateTime.Now : null
                },
                new ViewModels.Achievement
                {
                    Title = "Questioner",
                    Description = "Ask 3 questions",
                    Icon = "❓",
                    Unlocked = userPosts >= 3,
                    UnlockedDate = userPosts >= 3 ? DateTime.Now : null
                },
                new ViewModels.Achievement
                {
                    Title = "Rising Star",
                    Description = "Reach level 5",
                    Icon = "⭐",
                    Unlocked = level >= 5,
                    UnlockedDate = level >= 5 ? DateTime.Now : null
                },
                new ViewModels.Achievement
                {
                    Title = "Expert",
                    Description = "Reach level 10",
                    Icon = "🏆",
                    Unlocked = level >= 10,
                    UnlockedDate = level >= 10 ? DateTime.Now : null
                }
            };

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
                HelpfulAnswers = 0, // TODO: Count from Answer model if it has "IsHelpful" field
                Achievements = achievements
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
            // Validate model
            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                    }
                }
                return View(model);
            }

            // Get current user
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");

            // Check if user has enough points (posting costs 10 points)
            if (user.Points < 10)
            {
                ModelState.AddModelError("", "You need at least 10 points to post a question. Earn more points by answering questions or requesting help.");
                return View(model);
            }

            // Map faculty string to category
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Faculty == model.Faculty);
            
            if (category == null)
            {
                // If category doesn't exist, create it
                category = new Category { Faculty = model.Faculty, Name = model.Faculty };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }

            // Create new Post
            var post = new Post
            {
                Title = model.Title,
                Content = model.Content,
                UserID = user.UserID,
                CategoryID = category.CategoryID,
                CreatedAt = DateTime.UtcNow,
                ViewsCount = 0,
                Upvotes = 0,
                IsDeleted = false
            };

            // Add post to database first to get PostID
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Process tags if provided
            if (!string.IsNullOrWhiteSpace(model.Tags))
            {
                // Split tags by comma or space
                var tagNames = model.Tags
                    .Split(new[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Distinct()
                    .Take(5); // Max 5 tags

                foreach (var tagName in tagNames)
                {
                    // Find or create tag
                    var tag = await _context.Tags
                        .FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());
                    
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName };
                        _context.Tags.Add(tag);
                        await _context.SaveChangesAsync();
                    }

                    // Create PostTag relationship
                    var postTag = new PostTag
                    {
                        PostID = post.PostID,
                        TagID = tag.TagID
                    };
                    _context.PostTags.Add(postTag);
                }
            }

            // Deduct 10 points from user
            user.Points -= 10;
            _context.Users.Update(user);

            // Save all changes
            await _context.SaveChangesAsync();

            // Redirect to Dashboard after successful post
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
        public async Task<IActionResult> PostAnswer(int postId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Answer content cannot be empty.";
                return RedirectToAction("SinglePost", new { id = postId });
            }

            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");

            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostID == postId);
            if (post == null) return RedirectToAction("Dashboard");

            var answer = new Answer
            {
                PostID = postId,
                UserID = user.UserID,
                Content = content.Trim(),
                CreatedAt = DateTime.UtcNow,
                Upvotes = 0,
                IsDeleted = false,
                IsAccepted = false
            };

            _context.Answers.Add(answer);

            // Award +5 points to answering user
            user.Points += 5;
            _context.Users.Update(user);

            await _context.SaveChangesAsync();

            return RedirectToAction("SinglePost", new { id = postId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpvoteAnswer(int answerId)
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();

            var answer = await _context.Answers
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AnswerID == answerId && !a.IsDeleted);

            if (answer == null) return NotFound();

            answer.Upvotes += 1;
            _context.Answers.Update(answer);

            // Award +10 points to the answer author
            if (answer.User != null)
            {
                answer.User.Points += 10;
                _context.Users.Update(answer.User);
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, upvotes = answer.Upvotes });
        }

        private async Task<User> GetCurrentUser()
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
    }
}
