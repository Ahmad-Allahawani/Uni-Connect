using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uni_Connect.Models;
using Uni_Connect.Services;

namespace Uni_Connect.Controllers
{

    public class AnswerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IPointService _pointService;
        private readonly IPostService _postService;
        private readonly INotificationService _notificationService;

        public AnswerController(ApplicationDbContext context, IWebHostEnvironment environment,
            IPointService pointService, IPostService postService, INotificationService notificationService)
        {
            _context = context;
            _environment = environment;
            _pointService = pointService;
            _postService = postService;
            _notificationService = notificationService;
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostAnswer(int postId, string content, IFormFile? ImageFile)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Answer content cannot be empty.";
                return RedirectToAction("SinglePost","Post", new { id = postId });
            }

            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login_Page", "Login");

            var answer = await _postService.PostAnswer(postId, content, user.UserID, ImageFile);
            if (answer == null) return RedirectToAction("Dashboard","Dashboard");

            return RedirectToAction("SinglePost","Post", new { id = postId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpvoteAnswer(int answerId)
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();

            var result = await _postService.UpvoteAnswer(answerId, user.UserID);
            if (result == null) return BadRequest(new { message = "Cannot upvote your own answer." });

            return Ok(new { success = true, voted = result.Value.voted, upvotes = result.Value.upvotes });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAnswer(int answerId, int postId, string content)
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();

            var answer = await _context.Answers.FirstOrDefaultAsync(a =>
                a.AnswerID == answerId &&
                a.UserID == user.UserID &&
                !a.IsDeleted);

            if (answer == null) return Forbid();

            answer.Content = content?.Trim() ?? answer.Content;

            await _context.SaveChangesAsync();

            return RedirectToAction("SinglePost","Post", new { id = postId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptAnswer(int answerId)
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();

            var answer = await _context.Answers
                .Include(a => a.Post)
                .FirstOrDefaultAsync(a => a.AnswerID == answerId && !a.IsDeleted);

            if (answer == null) return NotFound();


            if (answer.Post.UserID != user.UserID) return Forbid();


            if (answer.UserID == user.UserID)
            {
                return BadRequest(new
                {
                    message = "You cannot mark your own answer as best."
                });
            }


            if (answer.IsAccepted)
            {
                return BadRequest(new
                {
                    message = "This answer is already marked as best."
                });
            }


            bool hasPendingRequest = await _context.BestAnswerRequests.AnyAsync(r =>
                r.PostID == answer.PostID &&
                !r.IsApproved &&
                !r.IsRejected &&
                !r.IsDeleted);

            if (hasPendingRequest)
            {
                return BadRequest(new
                {
                    message = "There is already a pending Best Answer request for this post."
                });
            }

            _context.BestAnswerRequests.Add(new BestAnswerRequest
            {
                PostID = answer.PostID,
                AnswerID = answer.AnswerID,
                RequestedByUserID = user.UserID,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Best Answer request sent to admin for approval."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnswer(int answerId, int postId)
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();
            await _postService.DeleteAnswer(answerId, user.UserID);
            TempData["SuccessMessage"] = "Your answer has been deleted.";
            return RedirectToAction("SinglePost","Post", new { id = postId });
        }


        private async Task<User?> GetCurrentUser()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return null;
            int userId = int.Parse(userIdStr);
            return await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
        }
    }
}
