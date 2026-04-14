using UniConnect.API.DTOs;

namespace UniConnect.API.Services;

public interface IPostService
{
    Task<IEnumerable<PostDto>> GetAllPostsAsync();
    Task<IEnumerable<PostDto>> GetPostsByCategoryAsync(int faculty, int course);
    Task<PostDto?> GetPostByIdAsync(int id);
    Task<PostDto> CreatePostAsync(int userId, CreatePostDto dto);
    Task<bool> DeletePostAsync(int postId, int userId);
    Task<CommentDto> AddCommentAsync(int postId, int userId, CreateCommentDto dto);
    Task<IEnumerable<CommentDto>> GetPostCommentsAsync(int postId);
}
