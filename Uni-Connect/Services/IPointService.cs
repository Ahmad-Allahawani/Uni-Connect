using Uni_Connect.Models;

namespace Uni_Connect.Services
{
    public interface IPointService
    {
        Task AwardPoints(int userId, int amount, string title, string? detail = null, string icon = "🎯");
        Task<bool> DeductPoints(int userId, int amount, string title, string? detail = null, string icon = "❓");
        Task<int> GetUserPoints(int userId);
        Task<bool> AwardPointsOnce(int userId, int amount, string title, int postId,
                              string? detail = null, string icon = "💬",
                              int dailyCap = 30);
    }
}
