using System.ComponentModel.DataAnnotations;

namespace DentalHealthTracker.Models
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Mevcut Şifre")]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [Display(Name = "Yeni Şifre")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Display(Name = "Yeni Şifre (Tekrar)")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
        public string? ConfirmPassword { get; set; }
    }
} 