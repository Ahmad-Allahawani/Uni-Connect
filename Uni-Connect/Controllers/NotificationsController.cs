using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Uni_Connect.Models;
using Microsoft.EntityFrameworkCore;

[Authorize]
[Route("api/notifications")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public NotificationsController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var me = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var notifications = await _context.Notifications
            .Where(n => n.UserID == me && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new { n.NotificationID, n.Message, n.Type, n.RelatedID, n.CreatedAt })
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpPost("mark-read/{id}")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var me = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationID == id && n.UserID == me);

        if (notification == null) return NotFound();
        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return Ok();
    }
    [HttpPost("delete-all")]
    public async Task<IActionResult> DeleteAll()
    {
        var me = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var notifs = await _context.Notifications
            .Where(n => n.UserID == me)
            .ToListAsync();
        _context.Notifications.RemoveRange(notifs);
        await _context.SaveChangesAsync();
        return Ok();
    }
}