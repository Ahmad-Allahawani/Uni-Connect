namespace Uni_Connect.Services
{
    public interface INotificationService
    {
        Task CreateAsync(int recipientId, string message, string type, int? relatedId = null, int? actorUserId = null);
    }
}
