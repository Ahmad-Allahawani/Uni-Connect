using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Uni_Connect.Models;

namespace Uni_Connect.Controllers
{
    [Authorize]
    [Route("api/messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MessagesController(ApplicationDbContext context)
        {
            _context = context;
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
                    Time = m.SentAt.ToString("HH:mm")
                })
                .ToListAsync();

            return Ok(messages);
        }
    }
}