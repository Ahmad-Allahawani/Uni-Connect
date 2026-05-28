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

        public async Task<bool> AwardPointsOnce(int userId, int amount, string title, int postId,
                                         string? detail = null, string icon = "💬",
                                         int dailyCap = 30)
        {
            // Guard 1: already rewarded for answering THIS post?
            bool alreadyRewarded = await _context.PointsTransactions.AnyAsync(pt =>
                pt.UserID == userId &&
                pt.Title == title &&
                pt.Detail != null && pt.Detail.Contains($"postId:{postId}") &&
                !pt.IsDeleted);

            if (alreadyRewarded) return false;

            // Guard 2: daily cap — how many answer-points earned today?
            var today = DateTime.UtcNow.Date;
            int earnedToday = await _context.PointsTransactions
                .Where(pt => pt.UserID == userId &&
                             pt.Title == title &&
                             pt.CreatedAt >= today &&
                             !pt.IsDeleted)
                .SumAsync(pt => pt.Amount);

            if (earnedToday >= dailyCap) return false;

            // Award the points
            await AwardPoints(userId, amount, title, $"{detail} | postId:{postId}", icon);
            return true;
        }
    }
}
