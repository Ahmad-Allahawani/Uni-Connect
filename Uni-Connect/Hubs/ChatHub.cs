using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Uni_Connect.Models;
using Uni_Connect.Services;

namespace Uni_Connect.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public ChatHub(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public override async Task OnConnectedAsync()
        {
            var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.IsOnline = true;
                    user.LastSeenAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.IsOnline = false;
                    user.LastSeenAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        }

        public async Task SendMessage(string roomId, string message, string? imageUrl = null)
        {
            var senderId = int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var sessionId = int.Parse(roomId);

            var session = await _context.PrivateSessions.FindAsync(sessionId);
            if (session == null || (!session.IsActive) ||
                (session.StudentID != senderId && session.HelperID != senderId))
            {
                throw new HubException("Not authorized to send in this session.");
            }

            var newMessage = new Message
            {
                SessionID = sessionId,
                SenderID = senderId,
                MessageText = message,
                SentAt = DateTime.UtcNow,
                IsRead = false,
                ImageUrl = imageUrl
            };

            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();

            int recipientId = session.StudentID == senderId
                ? session.HelperID
                : session.StudentID;
            var sender = await _context.Users.FindAsync(senderId);
            await _notificationService.CreateAsync(
                recipientId,
                $"New message from {sender!.Name}",
                "NewMessage",
                newMessage.MessageID
            );

            await Clients.Group(roomId).SendAsync(
                "ReceiveMessage",
                Context.ConnectionId,
                senderId,
                message,
                newMessage.SentAt.ToString("HH:mm"),
                imageUrl
            );
        }
        public async Task LeaveRoom(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        }
    }
}