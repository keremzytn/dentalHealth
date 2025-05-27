namespace DentalHealthTracker.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendPasswordResetEmailAsync(string to, string resetLink);
        Task<bool> SendWelcomeEmailAsync(string to, string userName);
    }
}