using Uni_Connect.Models;

public class NotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(int recipientId, string message, string type, int? relatedId = null)
    {
        var notification = new Notification
        {
            UserID = recipientId,
            Message = message,
            Type = type,
            RelatedID = relatedId,
            CreatedAt = DateTime.UtcNow
        };
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }
}