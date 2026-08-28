using System.ComponentModel.DataAnnotations;

namespace Uni_Connect.ViewModels
{
    /// <summary>
    /// This ViewModel represents the data that the Register form sends to the server.
    /// Each property = one form field.
    /// The [Required], [EmailAddress], etc. are "Data Annotations" — they define validation rules.
    /// If a rule is broken, ASP.NET automatically shows the ErrorMessage to the user.
    /// </summary>
    public class RegisterViewModel
    {
        // --- Full Name field ---
        // [Required] = this field cannot be empty
        // [StringLength(100)] = maximum 100 characters
        [Required(ErrorMessage = "Please enter your full name")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        // --- University Email field ---
        // [EmailAddress] = must be a valid email format (has @ and domain)
        // We also check in the controller that it ends with @philadelphia.edu.jo
        [Required(ErrorMessage = "Please enter your university email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "University Email")]
        public string Email { get; set; }

        // --- Faculty dropdown ---
        [Required(ErrorMessage = "Please select your faculty")]
        [Display(Name = "Faculty")]
        public string Faculty { get; set; }

        // --- Year of Study dropdown ---
        [Required(ErrorMessage = "Please select your year of study")]
        [Display(Name = "Year of Study")]
        public string YearOfStudy { get; set; }

        // --- Password field ---
        // [MinLength(8)] = at least 8 characters (matches your doc: "Minimum 8 characters")
        // [DataType(DataType.Password)] = tells the browser to show dots instead of text
        [Required(ErrorMessage = "Please enter a password")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character (!@#$%^&*)")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        // --- Confirm Password field ---
        // [Compare("Password")] = must match the Password field exactly
        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}
