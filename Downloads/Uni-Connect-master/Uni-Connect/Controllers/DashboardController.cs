using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Uni_Connect.Models;

namespace Uni_Connect.Controllers
{
    [Authorize] // Only logged-in users can access the dashboard
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            // 1. Get current user's ID from the cookie
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Login");

            int userId = int.Parse(userIdString);

            // 2. Fetch user data (for points and name)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null) return RedirectToAction("Login", "Login");

            // 3. Fetch recent posts (Academic questions)
            // We include User and Category navigation properties so we can show author names and faculty tags
            var recentPosts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Answers)
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .ToListAsync();

            // 4. Fetch top contributors (Leaderboard snapshot)
            var topContributors = await _context.Users
                .OrderByDescending(u => u.Points)
                .Take(5)
                .ToListAsync();

            // 5. Count unread notifications (placeholder for now)
            int notificationsCount = await _context.Notifications
                .CountAsync(n => n.UserID == userId && !n.IsRead);

            // 6. Create the ViewModel
            var viewModel = new DashboardViewModel
            {
                CurrentUser = user,
                RecentPosts = recentPosts,
                TopContributors = topContributors,
                UnreadNotificationsCount = notificationsCount
            };

            return View(viewModel);
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult Notifications()
        {
            return View();
        }

        public IActionResult Leaderboard()
        {
            return View();
        }
    }
}
