using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniConnect.Web.Data;
using UniConnect.Web.Models;

namespace UniConnect.Web.Controllers
{
    public class SessionsController : Controller
    {
        private readonly UniConnectDbContext _context;
        private readonly ILogger<SessionsController> _logger;

        public SessionsController(ILogger<SessionsController> logger, UniConnectDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: Sessions
        [Authorize]
        public async Task<IActionResult> Index(string? status = null)
        {
            var userId = User.GetUserId();
            
            var query = _context.PrivateSessions
                .Include(s => s.Student)
                .Include(s => s.Tutor)
                .Include(s => s.Messages)
                .Where(s => s.StudentId == userId || s.TutorId == userId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(s => s.Status == status);
            }

            var sessions = await query
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return View(sessions);
        }

        // GET: Sessions/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.GetUserId();
            
            var session = await _context.PrivateSessions
                .Include(s => s.Student)
                .Include(s => s.Tutor)
                .Include(s => s.Messages)
                    .ThenInclude(m => m.Sender)
                .Include(s => s.Ratings)
                .FirstOrDefaultAsync(s => s.Id == id && (s.StudentId == userId || s.TutorId == userId));

            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }

        // POST: Sessions/Start/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Start(int id)
        {
            var session = await _context.PrivateSessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }

            var userId = User.GetUserId();
            
            // Only tutor can start the session
            if (session.TutorId != userId)
            {
                return Forbid();
            }

            if (session.Status == "Pending")
            {
                session.Status = "Active";
                session.StartedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Sessions/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Complete(int id)
        {
            var session = await _context.PrivateSessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }

            var userId = User.GetUserId();
            
            // Only student can complete the session
            if (session.StudentId != userId)
            {
                return Forbid();
            }

            if (session.Status == "Active")
            {
                session.Status = "Completed";
                session.CompletedAt = DateTime.Now;

                // Award points to tutor
                var tutor = await _context.Users.FindAsync(session.TutorId);
                if (tutor != null)
                {
                    tutor.Points += session.PointsEarned;
                    tutor.TotalSessions++;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Sessions/SendMessage/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> SendMessage(int sessionId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction(nameof(Details), new { id = sessionId });
            }

            var session = await _context.PrivateSessions.FindAsync(sessionId);
            if (session == null)
            {
                return NotFound();
            }

            var userId = User.GetUserId();
            
            // Only participants can send messages
            if (session.StudentId != userId && session.TutorId != userId)
            {
                return Forbid();
            }

            var receiverId = session.StudentId == userId ? session.TutorId : session.StudentId;

            var message = new Message
            {
                SessionId = sessionId,
                SenderId = userId,
                ReceiverId = receiverId,
                Content = content,
                CreatedAt = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = sessionId });
        }

        // POST: Sessions/Rate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Rate(int sessionId, int rating, string? comment)
        {
            if (rating < 1 || rating > 5)
            {
                return RedirectToAction(nameof(Details), new { id = sessionId });
            }

            var session = await _context.PrivateSessions.FindAsync(sessionId);
            if (session == null)
            {
                return NotFound();
            }

            var userId = User.GetUserId();
            
            // Only participants can rate
            if (session.StudentId != userId && session.TutorId != userId)
            {
                return Forbid();
            }

            // Check if already rated
            var existingRating = await _context.SessionRatings
                .FirstOrDefaultAsync(r => r.SessionId == sessionId && r.UserId == userId);

            if (existingRating == null)
            {
                var ratingObj = new SessionRating
                {
                    SessionId = sessionId,
                    UserId = userId,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.Now
                };

                _context.SessionRatings.Add(ratingObj);

                // Update tutor's rating
                var tutorId = session.TutorId == userId ? session.TutorId : session.StudentId;
                var tutor = await _context.Users.FindAsync(tutorId);
                if (tutor != null)
                {
                    var totalRatings = await _context.SessionRatings
                        .Where(r => r.Session != null && r.Session.TutorId == tutorId)
                        .CountAsync();
                    
                    tutor.TotalRatings = totalRatings;
                    tutor.Rating = await _context.SessionRatings
                        .Where(r => r.Session != null && r.Session.TutorId == tutorId)
                        .AverageAsync(r => (double?)r.Rating) ?? 0;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = sessionId });
        }

        // POST: Sessions/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Cancel(int id)
        {
            var session = await _context.PrivateSessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }

            var userId = User.GetUserId();
            
            // Only participants can cancel
            if (session.StudentId != userId && session.TutorId != userId)
            {
                return Forbid();
            }

            if (session.Status == "Pending" || session.Status == "Active")
            {
                session.Status = "Cancelled";
                
                // Refund points if cancelled before completion
                if (session.Status == "Pending")
                {
                    var student = await _context.Users.FindAsync(session.StudentId);
                    if (student != null)
                    {
                        student.Points += session.PointsEarned;
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
