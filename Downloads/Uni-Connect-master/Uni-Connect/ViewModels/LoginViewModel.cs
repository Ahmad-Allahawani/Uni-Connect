using System.ComponentModel.DataAnnotations;

namespace Uni_Connect.ViewModels
{
    /// <summary>
    /// This ViewModel represents the data that the Login form sends to the server.
    /// Only 2 fields: Email and Password.
    /// </summary>
    public class LoginViewModel
    {
        // --- University Email field ---
        [Required(ErrorMessage = "Please enter your university email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "University Email")]
        public string Email { get; set; }

        // --- Password field ---
        [Required(ErrorMessage = "Please enter your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}
