using Uni_Connect.Models;

namespace Uni_Connect.Services
{
    public interface IPointService
    {
        Task AwardPoints(int userId, int amount, string title, string? detail = null, string icon = "🎯");
        Task<bool> DeductPoints(int userId, int amount, string title, string? detail = null, string icon = "❓");
        Task<int> GetUserPoints(int userId);
    }
}
