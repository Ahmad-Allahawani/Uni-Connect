using Microsoft.AspNetCore.Mvc;
using Uni_Connect.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Uni_Connect.Controllers
{

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
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null) return RedirectToAction("Login_Page", "Login");

            return View(user);
        }
        public async Task<IActionResult> Profile()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");
            return View(user);
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
            return View(user);
        }
        public async Task<IActionResult> CreatePost()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");
            return View(user);
        }

        public async Task<IActionResult> Sessions()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");
            return View(user);
        }

        public async Task<IActionResult> Points()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");
            return View(user);
        }

        public async Task<IActionResult> ChatPage()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");
            return View(user);
        }

        public async Task<IActionResult> SinglePost()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");
            return View(user);
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
