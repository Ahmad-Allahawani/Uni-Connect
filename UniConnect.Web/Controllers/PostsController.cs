using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniConnect.Web.Data;
using UniConnect.Web.Models;

namespace UniConnect.Web.Controllers
{
    public class PostsController : Controller
    {
        private readonly UniConnectDbContext _context;
        private readonly ILogger<PostsController> _logger;

        public PostsController(ILogger<PostsController> logger, UniConnectDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: Posts
        public async Task<IActionResult> Index(string? category = null, string? search = null)
        {
            var query = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Comments)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category != null && p.Category.Name == category);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Title.Contains(search) || p.Content.Contains(search));
            }

            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.Categories = await _context.Categories
                .Where(c => c.Type == "Faculty")
                .ToListAsync();

            return View(posts);
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            // Increment view count
            post.ViewCount++;
            await _context.SaveChangesAsync();

            return View(post);
        }

        // GET: Posts/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories
                .Where(c => c.Type == "Faculty" || c.Type == "Course")
                .ToListAsync();

            return View();
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Title,Content,CategoryId")] Post post)
        {
            if (ModelState.IsValid)
            {
                var userId = User.GetUserId();
                post.UserId = userId;
                post.CreatedAt = DateTime.Now;

                _context.Add(post);
                await _context.SaveChangesAsync();

                // Update user post count
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.TotalPosts++;
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _context.Categories
                .Where(c => c.Type == "Faculty" || c.Type == "Course")
                .ToListAsync();

            return View(post);
        }

        // POST: Posts/Comment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddComment(int postId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction(nameof(Details), new { id = postId });
            }

            var comment = new Comment
            {
                PostId = postId,
                UserId = User.GetUserId(),
                Content = content,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);

            // Update user comment count
            var user = await _context.Users.FindAsync(User.GetUserId());
            if (user != null)
            {
                user.TotalComments++;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = postId });
        }

        // POST: Posts/Vote/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Vote(int id, bool isUpvote)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            var userId = User.GetUserId();
            var existingVote = await _context.PostVotes
                .FirstOrDefaultAsync(v => v.PostId == id && v.UserId == userId);

            if (existingVote != null)
            {
                if (existingVote.IsUpvote == isUpvote)
                {
                    // Remove vote
                    _context.PostVotes.Remove(existingVote);
                    if (isUpvote) post.Upvotes--;
                    else post.Downvotes--;
                }
                else
                {
                    // Change vote
                    existingVote.IsUpvote = isUpvote;
                    if (isUpvote)
                    {
                        post.Upvotes++;
                        post.Downvotes--;
                    }
                    else
                    {
                        post.Downvotes++;
                        post.Upvotes--;
                    }
                }
            }
            else
            {
                var vote = new PostVote
                {
                    PostId = id,
                    UserId = userId,
                    IsUpvote = isUpvote,
                    CreatedAt = DateTime.Now
                };
                _context.PostVotes.Add(vote);
                if (isUpvote) post.Upvotes++;
                else post.Downvotes++;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Posts/MarkBestAnswer/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> MarkBestAnswer(int commentId)
        {
            var comment = await _context.Comments
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null || comment.Post == null)
            {
                return NotFound();
            }

            // Only post owner can mark best answer
            if (comment.Post.UserId != User.GetUserId())
            {
                return Forbid();
            }

            // Unmark previous best answer
            var previousBest = await _context.Comments
                .Where(c => c.PostId == comment.PostId && c.IsBestAnswer)
                .ToListAsync();

            foreach (var best in previousBest)
            {
                best.IsBestAnswer = false;
            }

            comment.IsBestAnswer = true;
            comment.Post.IsResolved = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = comment.PostId });
        }
    }
}
