using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uni_Connect.Models;
using Uni_Connect.Services;

namespace Uni_Connect.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IPointService _pointService;
        private readonly IPostService _postService;
        private readonly INotificationService _notificationService;

        public SearchController(ApplicationDbContext context, IWebHostEnvironment environment,
            IPointService pointService, IPostService postService, INotificationService notificationService)
        {
            _context = context;
            _environment = environment;
            _pointService = pointService;
            _postService = postService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Search(string? q, string tab = "posts")
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");

            var postsQuery = _context.Posts
                .Where(p => !p.IsDeleted)
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Answers.Where(a => !a.IsDeleted))
                .AsQueryable();

            var usersQuery = _context.Users
                .Where(u => !u.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();

                postsQuery = postsQuery.Where(p =>
                        p.Title.Contains(q) ||
                        p.Content.Contains(q) ||
                        (p.Category != null && p.Category.Name.Contains(q)) ||
                        (p.PostTags != null && p.PostTags.Any(pt => pt.Tag.Name.Contains(q))) ||

                        // Search posts by author info
                        p.User.Name.Contains(q) ||
                        p.User.Username.Contains(q) ||
                        p.User.UniversityID.Contains(q) ||
                        p.User.Email.Contains(q));


                usersQuery = usersQuery.Where(u =>
                    u.Name.Contains(q) ||
                    u.Username.Contains(q) ||
                    u.UniversityID.Contains(q) ||
                    u.Email.Contains(q));
            }

            var posts = await postsQuery
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var users = await usersQuery
                .OrderBy(u => u.Name)
                .Take(30)
                .ToListAsync();

            ViewBag.Query = q;
            ViewBag.Results = posts;
            ViewBag.UserResults = users;
            ViewBag.CurrentTab = tab;

            return View(user);
        }
        [HttpGet]
        public async Task<IActionResult> SearchPostsJson(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 3)
                return Json(new { results = Array.Empty<object>() });

            q = q.Trim();

            var matches = await _context.Posts
                .Where(p => !p.IsDeleted && (
                    p.Title.Contains(q) ||
                    p.Content.Contains(q) ||
                    (p.PostTags != null && p.PostTags.Any(pt => pt.Tag.Name.Contains(q)) ||
                    p.Category != null && p.Category.Name.Contains(q))
                ))
                .Include(p => p.Answers)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new
                {
                    id = p.PostID,
                    title = p.Title,
                    faculty = p.Category != null ? p.Category.Name : "",
                    answers = p.Answers.Count(a => !a.IsDeleted),
                    upvotes = p.Upvotes,
                    solved = p.Answers.Any(a => a.IsAccepted && !a.IsDeleted)
                })
                .ToListAsync();

            return Json(new { results = matches });
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
