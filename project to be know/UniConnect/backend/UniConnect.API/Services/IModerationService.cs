namespace UniConnect.API.Services;

public interface IModerationService
{
    Task<bool> IsContentSafeAsync(string content);
    Task FlagPostAsync(int postId);
    Task<bool> ApproveContentAsync(int postId);
}
