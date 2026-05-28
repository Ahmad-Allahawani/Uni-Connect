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
        private readonly INotificationService _notificationService;

        public PostService(ApplicationDbContext context, IWebHostEnvironment environment,
            IPointService pointService, INotificationService notificationService)
        {
            _context = context;
            _environment = environment;
            _pointService = pointService;
            _notificationService = notificationService;
        }

        public async Task<Post?> CreatePost(CreatePostViewModel model, int userId)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Faculty == model.Faculty);
            if (category == null)
            {
                category = new Category { Faculty = model.Faculty, Name = model.Faculty };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }

            string? imageUrl = await SaveImage(model.ImageFile, "posts");

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

            // Bulk tag processing — one query for all existing tags
            if (!string.IsNullOrWhiteSpace(model.Tags))
            {
                var tagNames = model.Tags
                    .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim().ToLower())
                    .Where(t => t.Length > 0)
                    .Distinct()
                    .Take(5)
                    .ToList();

                var existingTags = await _context.Tags
                    .Where(t => tagNames.Contains(t.Name.ToLower()))
                    .ToListAsync();

                var postTagsToAdd = new List<PostTag>();

                foreach (var tagName in tagNames)
                {
                    var tag = existingTags.FirstOrDefault(t => t.Name.ToLower() == tagName);
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName };
                        _context.Tags.Add(tag);
                    }
                    postTagsToAdd.Add(new PostTag { Post = post, Tag = tag });
                }

                _context.PostTags.AddRange(postTagsToAdd);
                await _context.SaveChangesAsync();
            }

            return post;
        }

        public async Task<Answer?> PostAnswer(int postId, string content, int userId, IFormFile? imageFile)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostID == postId);
            if (post == null) return null;

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
            await _context.SaveChangesAsync(); // save answer FIRST

            // Award points after answer is confirmed saved
            await _pointService.AwardPointsOnce(
                    userId: answer.UserID,
                    amount: 5,
                    title: "Posted an Answer",
                    postId: postId,
                    detail: content.Length > 60 ? content[..60] + "…" : content,
                    icon: "💬",
                    dailyCap: 30   
                );

            // Notify post owner (only if answerer is not the post owner)
            if (post.UserID != userId)
            {
                try
                {
                    var answerer = await _context.Users.FindAsync(userId);
                    string answererName = answerer?.Name?.Split(' ').First() ?? "Someone";
                    await _notificationService.CreateAsync(
                        post.UserID,
                        $"{answererName} answered your question!",
                        "Answer",
                        post.PostID
                    );
                }
                catch { /* notification failure doesn't block answer */ }
            }

            return answer;
        }

        public async Task<(bool voted, int upvotes)?> UpvoteAnswer(int answerId, int userId)
        {
            var answer = await _context.Answers
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AnswerID == answerId && !a.IsDeleted);

            if (answer == null) return null;
            if (answer.UserID == userId) return null; // can't vote own answer

            var existing = await _context.AnswerVotes
                .FirstOrDefaultAsync(av => av.AnswerID == answerId && av.UserID == userId);

            if (existing != null)
            {
                // Toggle OFF — remove vote
                _context.AnswerVotes.Remove(existing);
                answer.Upvotes = Math.Max(0, answer.Upvotes - 1);
                await _context.SaveChangesAsync();
                return (false, answer.Upvotes);
            }

            // Toggle ON — add vote
            _context.AnswerVotes.Add(new AnswerVote { UserID = userId, AnswerID = answerId });
            answer.Upvotes += 1;

            if (answer.User != null)
            {
                await _pointService.AwardPoints(answer.User.UserID, 10,
                    "Received an Upvote", "Your answer was upvoted", "👍");

                try
                {
                    var voter = await _context.Users.FindAsync(userId);
                    string voterName = voter?.Name?.Split(' ').First() ?? "Someone";
                    await _notificationService.CreateAsync(
                        answer.UserID,
                        $"{voterName} upvoted your answer!",
                        "Upvote",
                        answer.PostID,
                        actorUserId: userId
                    );
                }
                catch { }
            }

            await _context.SaveChangesAsync();
            return (true, answer.Upvotes);
        }

        public async Task<(bool voted, int upvotes)?> UpvotePost(int postId, int userId)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PostID == postId && !p.IsDeleted);

            if (post == null) return null;
            if (post.UserID == userId) return null; // can't vote own post

            var existing = await _context.PostVotes
                .FirstOrDefaultAsync(pv => pv.PostID == postId && pv.UserID == userId);

            if (existing != null)
            {
                // Toggle OFF — remove vote
                _context.PostVotes.Remove(existing);
                post.Upvotes = Math.Max(0, post.Upvotes - 1);
                await _context.SaveChangesAsync();
                return (false, post.Upvotes);
            }

            // Toggle ON — add vote
            _context.PostVotes.Add(new PostVote { UserID = userId, PostID = postId });
            post.Upvotes += 1;

            if (post.User != null)
            {
                await _pointService.AwardPoints(post.User.UserID, 10,
                    "Received an Upvote", "Your question was upvoted", "👍");

                try
                {
                    var voter = await _context.Users.FindAsync(userId);
                    string voterName = voter?.Name?.Split(' ').First() ?? "Someone";
                    await _notificationService.CreateAsync(
                        post.UserID,
                        $"{voterName} upvoted your question!",
                        "Upvote",
                        post.PostID,
                        actorUserId: userId
                    );
                }
                catch { }
            }

            await _context.SaveChangesAsync();
            return (true, post.Upvotes);
        }

        public async Task<bool> DeletePost(int postId, int userId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostID == postId && p.UserID == userId && !p.IsDeleted);
            if (post == null) return false;

            post.IsDeleted = true;
            await _context.SaveChangesAsync();

            // Refund the 10-point post cost
            await _pointService.AwardPoints(userId, 10, "Post Removed", "Points refunded for removed post", "🗑️");

            return true;
        }

        public async Task<bool> DeleteAnswer(int answerId, int userId)
        {
            var answer = await _context.Answers
                .Include(a => a.Post)
                .FirstOrDefaultAsync(a => a.AnswerID == answerId && a.UserID == userId && !a.IsDeleted);
            if (answer == null) return false;

            answer.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<string?> SaveImage(IFormFile? file, string folder)
        {
            if (file == null || file.Length == 0) return null;

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024) return null;

            // Whitelist allowed extensions
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext)) return null;

            // Whitelist allowed MIME types
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant())) return null;

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + ext;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{folder}/{uniqueFileName}";
        }
    }
}
