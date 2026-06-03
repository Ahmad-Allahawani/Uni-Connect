using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Uni_Connect.Models;
using Uni_Connect.Services;

namespace Uni_Connect.Controllers
{
    [Authorize]
    public class SessionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public SessionController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        //public async Task<IActionResult> Sessions()
        //{
        //    var user = await GetCurrentUser();
        //    if (user == null) return RedirectToAction("Login_Page", "Login");

        //    var sessions = await _context.PrivateSessions
        //        .Where(s => s.StudentID == user.UserID || s.HelperID == user.UserID)
        //        .Include(s => s.Student)
        //        .Include(s => s.Helper)
        //        .Include(s => s.Messages)
        //        .ToListAsync();

        //    return View(sessions);
        //}
        public async Task<IActionResult> ChatPage()
        {
            var me = GetCurrentUserId();

            var activeSessions = await _context.PrivateSessions
                .Include(s => s.Student)
                .Include(s => s.Helper)
                .Include(s => s.Messages)
                .Where(s => (s.StudentID == me || s.HelperID == me) && s.IsActive && !s.IsDeleted)
                .ToListAsync();

            var incomingRequests = await _context.Requests
                .Include(r => r.Owner)
                .Include(r => r.Post)
                .Where(r => r.RecipientID == me && r.Status == "Pending" && !r.IsDeleted)
                .ToListAsync();
            var pendingRequests = await _context.Requests
                .Include(r => r.Recipient)
                .Include(r => r.Post)
                .Where(r => r.OwnerID == me && r.Status == "Pending" && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var history = await _context.PrivateSessions
                .Include(s => s.Student)
                .Include(s => s.Helper)
                .Where(s => (s.StudentID == me || s.HelperID == me) && !s.IsActive && !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var sessionNotifs = await _context.Notifications
               .Where(n => n.UserID == me && !n.IsRead && n.Type == "SessionRequest")
               .ToListAsync();
            sessionNotifs.ForEach(n => n.IsRead = true);
            await _context.SaveChangesAsync();

            ViewBag.ActiveSessions = activeSessions;
            ViewBag.IncomingRequests = incomingRequests;
            ViewBag.History = history;
            ViewBag.CurrentUserId = me;
            ViewBag.PendingRequests = pendingRequests;

            return View("~/Views/Session/ChatPage.cshtml");
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendRequest(int recipientId, int? postId, string description)
        {
            try
            {
                var me = GetCurrentUserId();

                if (me == recipientId)
                    return BadRequest(new { message = "You cannot request a session with yourself." });

                var existing = await _context.Requests.AnyAsync(r =>
                    r.OwnerID == me && r.RecipientID == recipientId &&
                    r.Status == "Pending" && !r.IsDeleted);

                if (existing)
                    return BadRequest(new { message = "You already have a pending request with this student." });

                var sessionAlreadyExists = await _context.PrivateSessions.AnyAsync(s =>
                    !s.IsDeleted && s.IsActive &&
                    ((s.StudentID == me && s.HelperID == recipientId) ||
                     (s.StudentID == recipientId && s.HelperID == me)));

                if (sessionAlreadyExists)
                    return BadRequest(new { message = "You already have an active session with this user." });

                var request = new Request
                {
                    OwnerID = me,
                    RecipientID = recipientId,
                    PostID = postId == 0 ? null : postId,
                    Description = description ?? "",
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Requests.Add(request);
                await _context.SaveChangesAsync();

                var sender = await _context.Users.FindAsync(me);
                if (sender != null)
                {
                    try
                    {
                        await _notificationService.CreateAsync(
                            recipientId,
                            $"{sender.Name} sent you a session request.",
                            "SessionRequest",
                            request.RequestID
                        );
                    }
                    catch { /* notification failure doesn't block request */ }
                }

                return Ok(new { message = "Request sent!" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again." });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptRequest(int requestId)
        {
            var me = GetCurrentUserId();

            var request = await _context.Requests
                .FirstOrDefaultAsync(r => r.RequestID == requestId &&
                                          r.RecipientID == me &&
                                          r.Status == "Pending" &&
                                          !r.IsDeleted);

            if (request == null) return NotFound();

            var sessionExists = await _context.PrivateSessions
                .AnyAsync(s => s.RequestID == requestId);

            if (sessionExists) return BadRequest("Session already exists.");

            request.Status = "Accepted";

            var session = new PrivateSession
            {
                RequestID = request.RequestID,
                StudentID = request.OwnerID,
                HelperID = request.RecipientID,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.PrivateSessions.Add(session);
            await _context.SaveChangesAsync();

            await _notificationService.CreateAsync(
                request.OwnerID,
                "Your session request was accepted!",
                "SessionRequest",
                session.PrivateSessionID
            );

            return Ok(new
            {
                sessionId = session.PrivateSessionID,
                redirectUrl = Url.Action("ChatPage", "Session", new { autoOpen = session.PrivateSessionID })
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineRequest(int requestId)
        {
            var me = GetCurrentUserId();

            var request = await _context.Requests
                .FirstOrDefaultAsync(r => r.RequestID == requestId &&
                                          r.RecipientID == me &&
                                          r.Status == "Pending" &&
                                          !r.IsDeleted);

            if (request == null) return NotFound();

            request.Status = "Declined";
            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseSession(int sessionId)
        {
            var me = GetCurrentUserId();

            var session = await _context.PrivateSessions
                .Include(s => s.Request)
                .Include(s => s.Student)
                .Include(s => s.Helper)
                .FirstOrDefaultAsync(s => s.PrivateSessionID == sessionId &&
                                          (s.StudentID == me || s.HelperID == me));

            if (session == null) return NotFound();

            session.IsActive = false;
            session.ClosedAt = DateTime.UtcNow;

            // Mark session's own request as Closed
            if (session.Request != null)
                session.Request.Status = "Closed";

            // Clean up any other lingering pending requests between these two users
            var lingering = await _context.Requests
                .Where(r => !r.IsDeleted && r.Status == "Pending" &&
                            ((r.OwnerID == session.StudentID && r.RecipientID == session.HelperID) ||
                             (r.OwnerID == session.HelperID && r.RecipientID == session.StudentID)))
                .ToListAsync();
            foreach (var req in lingering)
                req.Status = "Closed";

            await _context.SaveChangesAsync();

            // Notify both participants
            var duration = session.ClosedAt.HasValue
                ? (int)(session.ClosedAt.Value - session.CreatedAt).TotalMinutes
                : 0;
            var durationText = duration > 0 ? $" ({duration} min)" : "";

            var studentName = session.Student?.Name?.Split(' ').First() ?? "Student";
            var helperName  = session.Helper?.Name?.Split(' ').First()  ?? "Helper";

            try
            {
                await _notificationService.CreateAsync(
                    session.StudentID,
                    $"Your session with {helperName} has ended{durationText}. Tap to rate.",
                    "Session",
                    session.PrivateSessionID,
                    actorUserId: session.HelperID
                );
                await _notificationService.CreateAsync(
                    session.HelperID,
                    $"Your session with {studentName} has ended{durationText}.",
                    "Session",
                    session.PrivateSessionID,
                    actorUserId: session.StudentID
                );
            }
            catch { /* notification failure must not block the close response */ }

            return Ok(new { sessionId = session.PrivateSessionID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RateSession(int sessionId, int rating)
        {
            if (rating < 1 || rating > 5) return BadRequest(new { message = "Rating must be between 1 and 5." });

            var me = GetCurrentUserId();

            var session = await _context.PrivateSessions
                .FirstOrDefaultAsync(s => s.PrivateSessionID == sessionId &&
                                          (s.StudentID == me || s.HelperID == me) &&
                                          !s.IsActive);

            if (session == null) return NotFound();

            session.Rating = rating;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
        [HttpGet]
        public async Task<IActionResult> SearchStudents(string q)
        {
            var me = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
                return Json(new { results = Array.Empty<object>() });

            q = q.Trim();

            var students = await _context.Users
                .Where(u =>
                    !u.IsDeleted &&
                    u.UserID != me &&
                    (
                        u.Name.Contains(q) ||
                        u.Email.Contains(q) ||
                        u.Faculty.Contains(q)
                    ))
                .OrderBy(u => u.Name)
                .Take(10)
                .Select(u => new
                {
                    id = u.UserID,
                    name = u.Name,
                    email = u.Email,
                    faculty = u.Faculty,
                    year = u.YearOfStudy,
                    profileImage = u.ProfileImageUrl
                })
                .ToListAsync();

            return Json(new { results = students });
        }

        private async Task<User?> GetCurrentUser()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return null;
            int userId = int.Parse(userIdStr);
            return await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
        }


        private int GetCurrentUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}