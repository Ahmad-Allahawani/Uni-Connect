using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uni_Connect.Models;
using Uni_Connect.Services;
using Uni_Connect.ViewModels;

namespace Uni_Connect.Controllers
{
    public class PointsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IPointService _pointService;
        private readonly IPostService _postService;
        private readonly INotificationService _notificationService;

        public PointsController(ApplicationDbContext context, IWebHostEnvironment environment,
            IPointService pointService, IPostService postService, INotificationService notificationService)
        {
            _context = context;
            _environment = environment;
            _pointService = pointService;
            _postService = postService;
            _notificationService = notificationService;
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

        [HttpGet("/api/user/points")]
        public async Task<IActionResult> GetUserPoints()
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();
            int level = Math.Min(user.Points / 500 + 1, 10);
            int toNext = Math.Max(0, level * 500 - user.Points);
            return Ok(new { points = user.Points, level, toNext });
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
