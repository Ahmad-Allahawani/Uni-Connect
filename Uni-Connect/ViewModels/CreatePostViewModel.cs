using System.ComponentModel.DataAnnotations;

namespace Uni_Connect.ViewModels
{
    /// <summary>
    /// ViewModel for creating a new academic post/question.
    /// Used for the 3-step post creation wizard.
    /// 
    /// Business Rules:
    /// - Posting a question costs 10 points (FR5 - Points System)
    /// - Title must be specific and clear (min 10 chars)
    /// - Content must explain the problem in detail (min 50 chars)
    /// - Faculty and Course code are required for categorization
    /// - Tags are optional, max 5 per post (FR6 - Search & Discovery)
    /// </summary>
    public class CreatePostViewModel
    {
        // --- Title field ---
        [Required(ErrorMessage = "Question title is required")]
        [StringLength(150, MinimumLength = 10, 
            ErrorMessage = "Title must be between 10 and 150 characters")]
        [Display(Name = "Question Title")]
        public string Title { get; set; }

        // --- Content/Description field ---
        [Required(ErrorMessage = "Question description is required")]
        [StringLength(2000, MinimumLength = 50,
            ErrorMessage = "Description must be between 50 and 2000 characters")]
        [Display(Name = "Question Description")]
        public string Content { get; set; }

        // --- Faculty field (dropdown) ---
        [Required(ErrorMessage = "Please select your faculty")]
        [Display(Name = "Faculty")]
        public string Faculty { get; set; }

        // --- Course code field ---
        [Required(ErrorMessage = "Course code is required")]
        [StringLength(10, MinimumLength = 2,
            ErrorMessage = "Course code must be between 2 and 10 characters")]
        [Display(Name = "Course Code")]
        public string CourseCode { get; set; }

        // --- Tags field (comma-separated, optional) ---
        [StringLength(200)]
        [Display(Name = "Tags")]
        public string Tags { get; set; }

        // --- Category ID (selected from faculty dropdown internally) ---
        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        // --- Image Upload ---
        [Display(Name = "Attachment")]
        public IFormFile? ImageFile { get; set; }

        // --- Timestamp (set server-side) ---
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
