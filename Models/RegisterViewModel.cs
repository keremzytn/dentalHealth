using System.ComponentModel.DataAnnotations;

namespace DentalHealthTracker.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parola zorunludur")]
        [StringLength(100, ErrorMessage = "Parola en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Parolalar eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doğum tarihi zorunludur")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }
    }
} 