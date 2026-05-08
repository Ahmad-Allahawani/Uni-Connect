using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Uni_Connect.Models;

namespace Uni_Connect.Controllers
{
    [Authorize]
    public class SessionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public SessionController(ApplicationDbContext context, NotificationService notificationService)
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

            return View("~/Views/Session/ChatPage.cshtml");
        }

       
        [HttpPost]
        public async Task<IActionResult> SendRequest(int recipientId, int postId, string description)
        {
            var me = GetCurrentUserId();

            if (me == recipientId)
                return BadRequest("You cannot request a session with yourself.");

            // prevent duplicate pending request
            var existing = await _context.Requests.AnyAsync(r =>
                r.OwnerID == me && r.RecipientID == recipientId &&
                r.Status == "Pending" && !r.IsDeleted);

            if (existing)
                return BadRequest("You already have a pending request with this student.");

            var sessionAlreadyExists = await _context.PrivateSessions.AnyAsync(s =>
                !s.IsDeleted && s.IsActive &&
                ((s.StudentID == me && s.HelperID == recipientId) ||
                 (s.StudentID == recipientId && s.HelperID == me)));

            if (sessionAlreadyExists)
                return BadRequest("You already have an active session with this user.");
            var request = new Request
            {
                OwnerID = me,
                RecipientID = recipientId,
                PostID = postId,
                Description = description,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            
            var sender = await _context.Users.FindAsync(me);
            await _notificationService.CreateAsync(
                recipientId,
                $"{sender!.Name} sent you a session request.",
                "SessionRequest",
                request.RequestID
            );

            return Ok(new { message = "Request sent!" });
        }

        
        [HttpPost]
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
                redirectUrl = Url.Action("ChatPage", "Session")
            });
        }

        
        [HttpPost]
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
        public async Task<IActionResult> CloseSession(int sessionId)
        {
            var me = GetCurrentUserId();

            var session = await _context.PrivateSessions
                .FirstOrDefaultAsync(s => s.PrivateSessionID == sessionId &&
                                          (s.StudentID == me || s.HelperID == me));

            if (session == null) return NotFound();

            session.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok();
        }

        private async Task<User?> GetCurrentUser()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return null;
            int userId = int.Parse(userIdStr);
            return await _context.Users
            .Include(u => u.Notifications)
            .FirstOrDefaultAsync(u => u.UserID == userId);
        }

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}