using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uni_Connect.Models;
using Uni_Connect.Services;

namespace Uni_Connect.Controllers
{
    public class LeaderboardController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IPointService _pointService;
        private readonly IPostService _postService;
        private readonly INotificationService _notificationService;

        public LeaderboardController(ApplicationDbContext context, IWebHostEnvironment environment,
            IPointService pointService, IPostService postService, INotificationService notificationService)
        {
            _context = context;
            _environment = environment;
            _pointService = pointService;
            _postService = postService;
            _notificationService = notificationService;
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

        private async Task<User?> GetCurrentUser()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return null;
            int userId = int.Parse(userIdStr);
            return await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
        }
    }
}
