using Uni_Connect.Models;
using Uni_Connect.ViewModels;

namespace Uni_Connect.Services
{
    public interface IPostService
    {
        Task<Post?> CreatePost(CreatePostViewModel model, int userId);
        Task<Answer?> PostAnswer(int postId, string content, int userId, IFormFile? imageFile);
        Task<(bool voted, int upvotes)?> UpvoteAnswer(int answerId, int userId);
        Task<(bool voted, int upvotes)?> UpvotePost(int postId, int userId);
        Task<bool> DeletePost(int postId, int userId);
        Task<bool> DeleteAnswer(int answerId, int userId);
    }
}
