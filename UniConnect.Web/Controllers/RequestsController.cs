using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniConnect.Web.Data;
using UniConnect.Web.Models;

namespace UniConnect.Web.Controllers
{
    public class RequestsController : Controller
    {
        private readonly UniConnectDbContext _context;
        private readonly ILogger<RequestsController> _logger;

        public RequestsController(ILogger<RequestsController> logger, UniConnectDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: Requests
        public async Task<IActionResult> Index(string? status = null)
        {
            var query = _context.AcademicRequests
                .Include(r => r.User)
                .Include(r => r.Category)
                .Include(r => r.Proposals)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            var requests = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        // GET: Requests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.AcademicRequests
                .Include(r => r.User)
                .Include(r => r.Category)
                .Include(r => r.Proposals)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // GET: Requests/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories
                .Where(c => c.Type == "Faculty" || c.Type == "Course")
                .ToListAsync();

            return View();
        }

        // POST: Requests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Title,Description,CategoryId,PointsOffered")] AcademicRequest request)
        {
            if (ModelState.IsValid)
            {
                var userId = User.GetUserId();
                var user = await _context.Users.FindAsync(userId);

                if (user == null || user.Points < request.PointsOffered)
                {
                    ModelState.AddModelError(string.Empty, "Insufficient points.");
                    ViewBag.Categories = await _context.Categories
                        .Where(c => c.Type == "Faculty" || c.Type == "Course")
                        .ToListAsync();
                    return View(request);
                }

                request.UserId = userId;
                request.CreatedAt = DateTime.Now;
                request.Status = "Open";

                // Deduct points
                user.Points -= request.PointsOffered;

                _context.Add(request);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _context.Categories
                .Where(c => c.Type == "Faculty" || c.Type == "Course")
                .ToListAsync();

            return View(request);
        }

        // POST: Requests/SubmitProposal/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> SubmitProposal(int requestId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return RedirectToAction(nameof(Details), new { id = requestId });
            }

            var request = await _context.AcademicRequests.FindAsync(requestId);
            if (request == null || request.Status != "Open")
            {
                return NotFound();
            }

            var proposal = new Proposal
            {
                RequestId = requestId,
                UserId = User.GetUserId(),
                Message = message,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.Proposals.Add(proposal);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = requestId });
        }

        // POST: Requests/AcceptProposal/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AcceptProposal(int proposalId)
        {
            var proposal = await _context.Proposals
                .Include(p => p.Request)
                .FirstOrDefaultAsync(p => p.Id == proposalId);

            if (proposal == null || proposal.Request == null)
            {
                return NotFound();
            }

            // Only request owner can accept
            if (proposal.Request.UserId != User.GetUserId())
            {
                return Forbid();
            }

            proposal.Status = "Accepted";
            proposal.Request.Status = "Accepted";

            // Create a private session
            var session = new PrivateSession
            {
                RequestId = proposal.Request.Id,
                StudentId = proposal.Request.UserId,
                TutorId = proposal.UserId,
                Subject = proposal.Request.Title,
                Description = proposal.Request.Description,
                Status = "Pending",
                PointsEarned = proposal.Request.PointsOffered,
                CreatedAt = DateTime.Now
            };

            _context.PrivateSessions.Add(session);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = proposal.RequestId });
        }

        // POST: Requests/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Cancel(int id)
        {
            var request = await _context.AcademicRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            // Only owner can cancel
            if (request.UserId != User.GetUserId())
            {
                return Forbid();
            }

            if (request.Status == "Open")
            {
                // Refund points
                var user = await _context.Users.FindAsync(request.UserId);
                if (user != null)
                {
                    user.Points += request.PointsOffered;
                }

                request.Status = "Cancelled";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
