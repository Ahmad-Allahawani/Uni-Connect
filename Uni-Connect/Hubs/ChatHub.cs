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

        public async Task SendMessage(string  roomId, string message)
        {
            var senderId = int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var newMessage = new Message
            {
                SessionID = int.Parse(roomId),
                SenderID = senderId,
                MessageText = message,
                SentAt = DateTime.UtcNow
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

    }
        
}
