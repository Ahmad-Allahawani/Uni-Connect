using System.ComponentModel.DataAnnotations;

namespace Uni_Connect.Models
{
    /// <summary>
    /// This ViewModel represents the data submitted when a user resets their password.
    /// It includes:
    /// - ResetToken: The token sent to user via email
    /// - NewPassword: The new password they want to set
    /// - ConfirmPassword: Password confirmation (must match NewPassword)
    /// </summary>
    public class ResetPasswordViewModel
    {
        // The reset token that was generated and sent to user's email
        // This proves they own the email address
        [Required(ErrorMessage = "Reset code is required")]
        public string ResetToken { get; set; }

        // --- New Password field ---
        // [MinLength(8)] = at least 8 characters
        // [RegularExpression] = must have uppercase, lowercase, number, special char
        [Required(ErrorMessage = "Please enter a password")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character (!@#$%^&*)")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        // --- Confirm Password field ---
        // [Compare("NewPassword")] = must match the NewPassword field exactly
        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}
