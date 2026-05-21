using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Uni_Connect.Models;
using Uni_Connect.Services;

namespace Uni_Connect.Controllers
{
    [Authorize]
    [Route("api/messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IWebHostEnvironment _environment;

        public MessagesController(ApplicationDbContext context, INotificationService notificationService, IWebHostEnvironment environment)
        {
            _context = context;
            _notificationService = notificationService;
            _environment = environment;
        }

        // GET /api/messages/{sessionId}
        [HttpGet("{sessionId}")]
        public async Task<IActionResult> GetMessages(int sessionId)
        {
            var me = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // verify caller is part of this session
            var session = await _context.PrivateSessions
                .FirstOrDefaultAsync(s => s.PrivateSessionID == sessionId &&
                                          (s.StudentID == me || s.HelperID == me) &&
                                          !s.IsDeleted);

            if (session == null) return Forbid();

            var messages = await _context.Messages
                .Where(m => m.SessionID == sessionId && !m.IsDeleted)
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    m.MessageID,
                    m.SenderID,
                    m.MessageText,
                    m.ImageUrl,
                    Time = m.SentAt.ToString("HH:mm")
                })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int sessionId, string messageText)
        {
            var me = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var session = await _context.PrivateSessions
                .FirstOrDefaultAsync(s => s.PrivateSessionID == sessionId &&
                    (s.StudentID == me || s.HelperID == me) && !s.IsDeleted);

            if (session == null) return Forbid();

            var message = new Message
            {
                SessionID = sessionId,
                SenderID = me,
                MessageText = messageText,
                SentAt = DateTime.UtcNow
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // notify the OTHER person in the session
            int recipientId = session.StudentID == me ? session.HelperID : session.StudentID;
            var sender = await _context.Users.FindAsync(me);
            await _notificationService.CreateAsync(
                recipientId,
                $"New message from {sender!.Name}",
                "NewMessage",
                message.MessageID
            );

            return Ok(new { messageId = message.MessageID });
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadChatImage(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file");
            if (file.Length > 5 * 1024 * 1024) return BadRequest("File too large (max 5MB)");

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var allowedMime = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowed.Contains(ext) || !allowedMime.Contains(file.ContentType.ToLowerInvariant()))
                return BadRequest("Invalid file type");

            var folder = Path.Combine(_environment.WebRootPath, "uploads", "messages");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid().ToString() + ext;
            var path = Path.Combine(folder, fileName);
            using var fs = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(fs);

            return Ok(new { imageUrl = $"/uploads/messages/{fileName}" });
        }
    }
}