using Microsoft.EntityFrameworkCore;
using Uni_Connect.Models;

namespace Uni_Connect.Services
{
    public class PointService : IPointService
    {
        private readonly ApplicationDbContext _context;

        public PointService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AwardPoints(int userId, int amount, string title, string? detail = null, string icon = "🎯")
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null) return;

            user.Points += amount;
            
            _context.PointsTransactions.Add(new PointsTransaction
            {
                UserID = userId,
                Title = title,
                Detail = detail,
                Amount = amount,
                Icon = icon,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeductPoints(int userId, int amount, string title, string? detail = null, string icon = "❓")
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null || user.Points < amount) return false;

            user.Points -= amount;

            _context.PointsTransactions.Add(new PointsTransaction
            {
                UserID = userId,
                Title = title,
                Detail = detail,
                Amount = -amount,
                Icon = icon,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUserPoints(int userId)
        {
            return await _context.Users
                .Where(u => u.UserID == userId)
                .Select(u => u.Points)
                .FirstOrDefaultAsync();
        }
    }
}
