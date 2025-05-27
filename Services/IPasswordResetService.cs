using System.Threading.Tasks;
using DentalHealthTracker.Models;

namespace DentalHealthTracker.Services
{
    public interface IPasswordResetService
    {
        Task<ServiceResult> CreatePasswordResetTokenAsync(string email);
        Task<ServiceResult> ValidateTokenAsync(string token);
        Task<ServiceResult> ResetPasswordAsync(string token, string newPassword);
        Task CleanupExpiredTokensAsync();
    }
}