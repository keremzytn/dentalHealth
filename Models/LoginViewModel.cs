using System.ComponentModel.DataAnnotations;

namespace DentalHealthTracker.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parola zorunludur")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
} 