using System.ComponentModel.DataAnnotations;

namespace Uni_Connect.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Please enter your university email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "University Email")]
        public string Email { get; set; }
    }
}
