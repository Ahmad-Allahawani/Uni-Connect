using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniConnect.Web.Data;
using UniConnect.Web.Models;

namespace UniConnect.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UniConnectDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ILogger<AdminController> logger, UniConnectDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalPosts = await _context.Posts.CountAsync(),
                TotalRequests = await _context.AcademicRequests.CountAsync(),
                TotalSessions = await _context.PrivateSessions.CountAsync(),
                TotalReports = await _context.Reports.CountAsync(),
                PendingReports = await _context.Reports.CountAsync(r => r.Status == "Pending"),
                RecentUsers = await _context.Users.OrderByDescending(u => u.CreatedAt).Take(10).ToListAsync(),
                RecentPosts = await _context.Posts.OrderByDescending(p => p.CreatedAt).Take(10).ToListAsync(),
                RecentReports = await _context.Reports.Include(r => r.Reporter).Include(r => r.ReportedUser)
                    .OrderByDescending(r => r.CreatedAt).Take(10).ToListAsync()
            };

            return View(model);
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users(string? search = null, string? role = null)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.UniversityId.Contains(search) || 
                    u.FirstName.Contains(search) || u.LastName.Contains(search) ||
                    u.Email.Contains(search));
            }

            if (!string.IsNullOrEmpty(role))
            {
                // Filter by role - we need to check if user has the role
                // This is simplified; in production you'd check Identity
            }

            var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
            return View(users);
        }

        // GET: Admin/Users/Details/5
        public async Task<IActionResult> UserDetails(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Posts)
                .Include(u => u.Comments)
                .Include(u => u.Requests)
                .Include(u => u.SessionsAsTutor)
                .Include(u => u.ReportsMade)
                .Include(u => u.ReportsReceived)
                .Include(u => u.Achievements)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/Users/ToggleVerification/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVerification(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsVerified = !user.IsVerified;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(UserDetails), new { id });
        }

        // POST: Admin/Users/Ban/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(UserDetails), new { id });
        }

        // POST: Admin/Users/Unban/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnbanUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(UserDetails), new { id });
        }

        // GET: Admin/Posts
        public async Task<IActionResult> Posts(string? search = null, string? category = null)
        {
            var query = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Title.Contains(search) || p.Content.Contains(search));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category != null && p.Category.Name == category);
            }

            var posts = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return View(posts);
        }

        // GET: Admin/Posts/Details/5
        public async Task<IActionResult> PostDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Admin/Posts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Posts));
        }

        // GET: Admin/Reports
        public async Task<IActionResult> Reports(string? status = null)
        {
            var query = _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            var reports = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
            return View(reports);
        }

        // GET: Admin/Reports/Details/5
        public async Task<IActionResult> ReportDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: Admin/Reports/Resolve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveReport(int id, string action)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            report.Status = action == "Approve" ? "Resolved" : "Dismissed";
            report.ReviewedById = User.GetUserId();
            report.ReviewedAt = DateTime.Now;

            // If report is approved, take action on the reported user/content
            if (action == "Approve" && report.ReportedUserId != null)
            {
                var user = await _context.Users.FindAsync(report.ReportedUserId);
                if (user != null)
                {
                    user.Streak = 0; // Reset streak as a warning
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ReportDetails), new { id });
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .Include(c => c.SubCategories)
                .Where(c => c.ParentId == null)
                .ToListAsync();

            return View(categories);
        }

        // GET: Admin/Categories/Create
        public async Task<IActionResult> CreateCategory()
        {
            ViewBag.Categories = await _context.Categories
                .Where(c => c.Type == "Faculty")
                .ToListAsync();

            return View();
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory([Bind("Name,Type,ParentId")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Categories));
            }

            ViewBag.Categories = await _context.Categories
                .Where(c => c.Type == "Faculty")
                .ToListAsync();

            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Categories));
        }

        // GET: Admin/Statistics
        public async Task<IActionResult> Statistics()
        {
            var model = new AdminStatisticsViewModel
            {
                UserStats = await GetUserStatsAsync(),
                PostStats = await GetPostStatsAsync(),
                RequestStats = await GetRequestStatsAsync(),
                SessionStats = await GetSessionStatsAsync()
            };

            return View(model);
        }

        private async Task<UserStatsViewModel> GetUserStatsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var verifiedUsers = await _context.Users.CountAsync(u => u.IsVerified);
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);

            return new UserStatsViewModel
            {
                TotalUsers = totalUsers,
                VerifiedUsers = verifiedUsers,
                ActiveUsers = activeUsers,
                UnverifiedUsers = totalUsers - verifiedUsers
            };
        }

        private async Task<PostStatsViewModel> GetPostStatsAsync()
        {
            var totalPosts = await _context.Posts.CountAsync();
            var resolvedPosts = await _context.Posts.CountAsync(p => p.IsResolved);

            return new PostStatsViewModel
            {
                TotalPosts = totalPosts,
                ResolvedPosts = resolvedPosts,
                UnresolvedPosts = totalPosts - resolvedPosts
            };
        }

        private async Task<RequestStatsViewModel> GetRequestStatsAsync()
        {
            var totalRequests = await _context.AcademicRequests.CountAsync();
            var completedRequests = await _context.AcademicRequests.CountAsync(r => r.Status == "Completed");
            var openRequests = await _context.AcademicRequests.CountAsync(r => r.Status == "Open");

            return new RequestStatsViewModel
            {
                TotalRequests = totalRequests,
                CompletedRequests = completedRequests,
                OpenRequests = openRequests
            };
        }

        private async Task<SessionStatsViewModel> GetSessionStatsAsync()
        {
            var totalSessions = await _context.PrivateSessions.CountAsync();
            var completedSessions = await _context.PrivateSessions.CountAsync(s => s.Status == "Completed");
            var activeSessions = await _context.PrivateSessions.CountAsync(s => s.Status == "Active");

            return new SessionStatsViewModel
            {
                TotalSessions = totalSessions,
                CompletedSessions = completedSessions,
                ActiveSessions = activeSessions
            };
        }
    }

    // View Models
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalPosts { get; set; }
        public int TotalRequests { get; set; }
        public int TotalSessions { get; set; }
        public int TotalReports { get; set; }
        public int PendingReports { get; set; }
        public List<User> RecentUsers { get; set; } = new List<User>();
        public List<Post> RecentPosts { get; set; } = new List<Post>();
        public List<Report> RecentReports { get; set; } = new List<Report>();
    }

    public class AdminStatisticsViewModel
    {
        public UserStatsViewModel UserStats { get; set; } = new();
        public PostStatsViewModel PostStats { get; set; } = new();
        public RequestStatsViewModel RequestStats { get; set; } = new();
        public SessionStatsViewModel SessionStats { get; set; } = new();
    }

    public class UserStatsViewModel
    {
        public int TotalUsers { get; set; }
        public int VerifiedUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int UnverifiedUsers { get; set; }
    }

    public class PostStatsViewModel
    {
        public int TotalPosts { get; set; }
        public int ResolvedPosts { get; set; }
        public int UnresolvedPosts { get; set; }
    }

    public class RequestStatsViewModel
    {
        public int TotalRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int OpenRequests { get; set; }
    }

    public class SessionStatsViewModel
    {
        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }
        public int ActiveSessions { get; set; }
    }
}
