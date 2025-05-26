using System.Threading.Tasks;
using DentalHealthTracker.Models;

namespace DentalHealthTracker.Services
{
    public interface IUserService
    {
        Task<ServiceResult> ValidateUserAsync(string email, string password);
        Task<ServiceResult> RegisterAsync(RegisterViewModel model);
        Task<User?> GetUserByEmailAsync(string email);
        Task<ServiceResult> UpdateProfileAsync(string email, ProfileViewModel model);
        Task<ServiceResult> ChangePasswordAsync(string email, ChangePasswordViewModel model);
    }

    public class ServiceResult
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ServiceResult Success()
        {
            return new ServiceResult { Succeeded = true };
        }

        public static ServiceResult Failure(string message)
        {
            return new ServiceResult { Succeeded = false, Message = message };
        }
    }
} 