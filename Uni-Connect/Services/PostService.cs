using Microsoft.EntityFrameworkCore;
using Uni_Connect.Models;
using Uni_Connect.ViewModels;

namespace Uni_Connect.Services
{
    public class PostService : IPostService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IPointService _pointService;

        public PostService(ApplicationDbContext context, IWebHostEnvironment environment, IPointService pointService)
        {
            _context = context;
            _environment = environment;
            _pointService = pointService;
        }

        public async Task<Post?> CreatePost(CreatePostViewModel model, int userId)
        {
            // Map faculty to category
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Faculty == model.Faculty);
            if (category == null)
            {
                category = new Category { Faculty = model.Faculty, Name = model.Faculty };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }

            // Handling Image Upload
            string? imageUrl = await SaveImage(model.ImageFile, "posts");

            // Create Post
            var post = new Post
            {
                Title = model.Title,
                Content = model.Content,
                UserID = userId,
                CategoryID = category.CategoryID,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                ImageUrl = imageUrl
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Process tags
            if (!string.IsNullOrWhiteSpace(model.Tags))
            {
                var tagNames = model.Tags.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct().Take(5);
                foreach (var tagName in tagNames)
                {
                    var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower()) 
                              ?? new Tag { Name = tagName };
                    
                    if (tag.TagID == 0) 
                    {
                        _context.Tags.Add(tag);
                        await _context.SaveChangesAsync();
                    }

                    _context.PostTags.Add(new PostTag { PostID = post.PostID, TagID = tag.TagID });
                }
                await _context.SaveChangesAsync();
            }

            return post;
        }

        public async Task<Answer?> PostAnswer(int postId, string content, int userId, IFormFile? imageFile)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostID == postId);
            if (post == null) return null;

            // Handle Image Upload
            string? imageUrl = await SaveImage(imageFile, "answers");

            var answer = new Answer
            {
                PostID = postId,
                UserID = userId,
                Content = content.Trim(),
                CreatedAt = DateTime.UtcNow,
                Upvotes = 0,
                IsDeleted = false,
                IsAccepted = false,
                ImageUrl = imageUrl
            };

            _context.Answers.Add(answer);
            
            // Award points via PointService
            await _pointService.AwardPoints(userId, 5, "Answered a Question", 
                post.Title.Length > 25 ? post.Title.Substring(0, 25) + "..." : post.Title, "🤝");

            await _context.SaveChangesAsync();
            return answer;
        }

        public async Task<bool> UpvoteAnswer(int answerId, int userId)
        {
            var answer = await _context.Answers
                .Include(a => a.User)
                .Include(a => a.Post)
                .FirstOrDefaultAsync(a => a.AnswerID == answerId && !a.IsDeleted);

            if (answer == null) return false;

            answer.Upvotes += 1;
            
            // Award points to the answer author
            if (answer.User != null)
            {
                await _pointService.AwardPoints(answer.User.UserID, 10, "Received an Upvote", "Your answer was upvoted", "👍");
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<string?> SaveImage(IFormFile? file, string folder)
        {
            if (file == null || file.Length == 0) return null;

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{folder}/{uniqueFileName}";
        }
    }
}
