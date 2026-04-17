using Uni_Connect.Models;
using Uni_Connect.ViewModels;

namespace Uni_Connect.Services
{
    public interface IPostService
    {
        Task<Post?> CreatePost(CreatePostViewModel model, int userId);
        Task<Answer?> PostAnswer(int postId, string content, int userId, IFormFile? imageFile);
        Task<bool> UpvoteAnswer(int answerId, int userId);
    }
}
