using System.Text.RegularExpressions;

namespace UniConnect.Web.Services
{
    public interface IModerationService
    {
        ModerationResult ModerateText(string content);
        ModerationResult ModerateImage(string imageUrl);
        bool ContainsInappropriateContent(string content);
        List<string> GetInappropriateWords();
    }

    public class ModerationService : IModerationService
    {
        // List of inappropriate words/phrases (this would be expanded in production)
        private static readonly HashSet<string> InappropriateWords = new(StringComparer.OrdinalIgnoreCase)
        {
            // Add inappropriate words here for content moderation
            // This is a placeholder list - in production, this would be more comprehensive
        };

        // List of spam patterns
        private static readonly List<Regex> SpamPatterns = new()
        {
            new Regex(@"(?i)(buy now|click here|free money|winner|congratulations)", RegexOptions.Compiled),
            new Regex(@"(?i)(http|www\.)\s*[a-zA-Z0-9]+\.(com|net|org)", RegexOptions.Compiled),
            new Regex(@"(.)\1{5,}", RegexOptions.Compiled) // Repeated characters
        };

        public ModerationResult ModerateText(string content)
        {
            var result = new ModerationResult { IsApproved = true };

            if (string.IsNullOrWhiteSpace(content))
            {
                result.IsApproved = false;
                result.RejectionReason = "Content cannot be empty";
                return result;
            }

            // Check length
            if (content.Length < 3)
            {
                result.IsApproved = false;
                result.RejectionReason = "Content is too short";
                return result;
            }

            if (content.Length > 10000)
            {
                result.IsApproved = false;
                result.RejectionReason = "Content is too long";
                return result;
            }

            // Check for inappropriate words
            foreach (var word in InappropriateWords)
            {
                if (content.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    result.IsApproved = false;
                    result.RejectionReason = $"Content contains inappropriate language: {word}";
                    result.FlaggedWords.Add(word);
                    result.IsFlagged = true;
                }
            }

            // Check for spam patterns
            foreach (var pattern in SpamPatterns)
            {
                if (pattern.IsMatch(content))
                {
                    result.IsFlagged = true;
                    result.Warnings.Add("Content may contain spam");
                }
            }

            // Check for excessive caps
            int uppercaseCount = content.Count(c => char.IsUpper(c));
            if (content.Length > 20 && uppercaseCount > content.Length * 0.7)
            {
                result.IsFlagged = true;
                result.Warnings.Add("Content contains too many uppercase letters");
            }

            // Check for excessive punctuation
            if (content.Contains("!!!") || content.Contains("???"))
            {
                result.IsFlagged = true;
                result.Warnings.Add("Content contains excessive punctuation");
            }

            return result;
        }

        public ModerationResult ModerateImage(string imageUrl)
        {
            var result = new ModerationResult { IsApproved = true };

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                result.IsApproved = false;
                result.RejectionReason = "Image URL is required";
                return result;
            }

            // In a production environment, this would integrate with an image moderation API
            // For now, we'll do basic validation

            // Check if URL is valid
            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
            {
                result.IsApproved = false;
                result.RejectionReason = "Invalid image URL";
                return result;
            }

            // Check file extension
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(imageUrl).ToLowerInvariant();
            if (!validExtensions.Contains(extension))
            {
                result.IsApproved = false;
                result.RejectionReason = "Invalid image format. Supported formats: JPG, PNG, GIF, WebP";
                return result;
            }

            // Flag for review if URL contains suspicious patterns
            if (imageUrl.Contains("temp") || imageUrl.Contains("cache"))
            {
                result.IsFlagged = true;
                result.Warnings.Add("Image may need manual review");
            }

            return result;
        }

        public bool ContainsInappropriateContent(string content)
        {
            var result = ModerateText(content);
            return !result.IsApproved;
        }

        public List<string> GetInappropriateWords()
        {
            return InappropriateWords.ToList();
        }
    }

    public class ModerationResult
    {
        public bool IsApproved { get; set; } = true;
        public bool IsFlagged { get; set; }
        public string? RejectionReason { get; set; }
        public List<string> FlaggedWords { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}
