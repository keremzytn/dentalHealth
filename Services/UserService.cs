using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DentalHealthTracker.Data;
using DentalHealthTracker.Models;
using System.Security.Cryptography;
using System.Text;

namespace DentalHealthTracker.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult> ValidateUserAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            if (user == null)
                return ServiceResult.Failure("Kullanıcı bulunamadı.");

            if (!VerifyPassword(password, user.Password))
                return ServiceResult.Failure("Hatalı parola.");

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> RegisterAsync(RegisterViewModel model)
        {
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                return ServiceResult.Failure("Bu e-posta adresi zaten kullanılıyor.");

            var user = new User
            {
                Email = model.Email,
                Password = HashPassword(model.Password),
                FullName = model.FullName,
                BirthDate = DateTime.SpecifyKind(model.BirthDate, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return ServiceResult.Success();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        public async Task<ServiceResult> UpdateProfileAsync(string email, ProfileViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return ServiceResult.Failure("Kullanıcı bulunamadı.");

            user.Email = model.Email;
            user.FullName = model.FullName;
            if (model.BirthDate.HasValue)
            {
                user.BirthDate = DateTime.SpecifyKind(model.BirthDate.Value, DateTimeKind.Utc);
            }

            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> ChangePasswordAsync(string email, ChangePasswordViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return ServiceResult.Failure("Kullanıcı bulunamadı.");

            if (string.IsNullOrEmpty(model.CurrentPassword) || !VerifyPassword(model.CurrentPassword, user.Password))
                return ServiceResult.Failure("Mevcut şifre hatalı.");

            if (model.NewPassword != model.ConfirmPassword)
                return ServiceResult.Failure("Yeni şifreler eşleşmiyor.");

            user.Password = HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }
    }
} 