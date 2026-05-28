using Org.BouncyCastle.Cms;
using Uni_Connect.Models;

namespace Uni_Connect.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(int recipientId, string message, string type, int? relatedId = null, int? actorUserId = null)
        {
            var user = await _context.Users.FindAsync(recipientId);
            if (user == null) return;

            if (type == "Answer" && user?.NotifyOnAnswers == false) return;
            if (type == "SessionRequest" && user?.NotifyOnSessionRequests == false) return;

            var notification = new Notification
            {
                UserID = recipientId,
                Message = message,
                Type = type,
                RelatedID = relatedId,
                ActorUserID = actorUserId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}
