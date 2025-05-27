using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DentalHealthTracker.Data;
using DentalHealthTracker.Models;
using System.Security.Cryptography;
using System.Text;

namespace DentalHealthTracker.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<PasswordResetService> _logger;

        public PasswordResetService(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<PasswordResetService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ServiceResult> CreatePasswordResetTokenAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return ServiceResult.Failure("Kullanıcı bulunamadı.");

            // Eski token'ları geçersiz kıl
            var oldTokens = await _context.PasswordResetTokens
                .Where(t => t.UserId == user.Id && !t.IsUsed)
                .ToListAsync();

            foreach (var token in oldTokens)
            {
                token.IsUsed = true;
            }

            // Yeni token oluştur
            var newToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = GenerateToken(),
                ExpiryDate = DateTime.UtcNow.AddHours(1),
                IsUsed = false
            };

            await _context.PasswordResetTokens.AddAsync(newToken);
            await _context.SaveChangesAsync();

            // E-posta gönder
            var resetLink = $"http://localhost:5288/Account/ResetPassword?token={newToken.Token}";
            var emailSent = await _emailService.SendPasswordResetEmailAsync(email, resetLink);

            if (!emailSent)
            {
                _logger.LogError("Şifre sıfırlama e-postası gönderilemedi. Kullanıcı: {Email}", email);
                return ServiceResult.Failure("Şifre sıfırlama e-postası gönderilemedi.");
            }

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> ValidateTokenAsync(string token)
        {
            var resetToken = await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token);

            if (resetToken == null)
                return ServiceResult.Failure("Geçersiz token.");

            if (resetToken.IsUsed)
                return ServiceResult.Failure("Bu token daha önce kullanılmış.");

            if (resetToken.ExpiryDate < DateTime.UtcNow)
                return ServiceResult.Failure("Token süresi dolmuş.");

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> ResetPasswordAsync(string token, string newPassword)
        {
            var validationResult = await ValidateTokenAsync(token);
            if (!validationResult.Succeeded)
                return validationResult;

            var resetToken = await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token);

            resetToken.IsUsed = true;
            resetToken.User.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        private string GenerateToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var tokenBytes = new byte[32];
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes);
        }

        public async Task CleanupExpiredTokensAsync()
        {
            var expiredTokens = await _context.PasswordResetTokens
                .Where(t => t.ExpiryDate < DateTime.UtcNow || t.IsUsed)
                .ToListAsync();

            _context.PasswordResetTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Count} adet süresi dolmuş token temizlendi.", expiredTokens.Count);
        }
    }
}