namespace UniConnect.API.Services;

public interface IPointsService
{
    Task AddPointsAsync(int userId, int points, string reason, int? relatedPostId = null);
    Task<bool> DeductPointsAsync(int userId, int points, string reason);
    Task<int> GetUserPointsAsync(int userId);
    Task<bool> VerifyUserAsync(int userId);
}
