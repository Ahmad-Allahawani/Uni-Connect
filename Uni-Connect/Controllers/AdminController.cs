using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uni_Connect.Models;

namespace Uni_Connect.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> AdminDashboard()
        {
            var userCount = await _context.Users.IgnoreQueryFilters().CountAsync();
            var postCount = await _context.Posts.IgnoreQueryFilters().CountAsync();
            var reportCount = await _context.Reports.CountAsync(r => !r.IsResolved);

            ViewBag.UserCount = userCount;
            ViewBag.PostCount = postCount;
            ViewBag.ReportCount = reportCount;

            return View();
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Users.IgnoreQueryFilters().OrderByDescending(u => u.CreatedAt).ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserDelete(int id)
        {
            var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.UserID == id);
            if (user != null)
            {
                user.IsDeleted = !user.IsDeleted;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageUsers");
        }

        public async Task<IActionResult> ManageReports()
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return View(reports);
        }
    }
}
