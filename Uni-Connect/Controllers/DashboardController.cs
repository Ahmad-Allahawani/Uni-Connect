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
        public IActionResult Dashboard()
        {
            return View();
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
        public IActionResult CreatePost()
        {
            return View();
        }

        public IActionResult ChatPage()
        {
            return View();
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
