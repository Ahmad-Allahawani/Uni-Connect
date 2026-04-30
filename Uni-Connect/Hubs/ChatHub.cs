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

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
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