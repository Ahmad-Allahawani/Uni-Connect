using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Uni_Connect.Models;

namespace Uni_Connect.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;  

        public ChatHub(ApplicationDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task JoinRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        }

        public async Task SendMessage(string roomId, string message)
        {
            var senderId = int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var sessionId = int.Parse(roomId);

            // verify sender belongs to this session
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
                IsRead = false
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
                newMessage.SentAt.ToString("HH:mm")
            );
        }
        public async Task LeaveRoom(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        }
    }
}