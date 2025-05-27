using System.Net.Mail;
using DentalHealthTracker.Models;
using Microsoft.Extensions.Options;

namespace DentalHealthTracker.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    EnableSsl = true,
                    Credentials = new System.Net.NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword)
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                await client.SendMailAsync(message);
                _logger.LogInformation("E-posta başarıyla gönderildi. Alıcı: {To}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-posta gönderilirken hata oluştu. Alıcı: {To}", to);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string to, string resetLink)
        {
            var subject = "Şifre Sıfırlama";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 5px;'>
                    <div style='background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0;'>
                        <h2 style='margin: 0;'>Şifre Sıfırlama</h2>
                    </div>
                    <div style='padding: 20px; background-color: #f9f9f9;'>
                        <p style='color: #333; font-size: 16px; line-height: 1.5;'>Merhaba,</p>
                        <p style='color: #333; font-size: 16px; line-height: 1.5;'>Şifrenizi sıfırlamak için aşağıdaki butona tıklayın:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' style='background-color: #4CAF50; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: bold; display: inline-block;'>Şifremi Sıfırla</a>
                        </div>
                        <p style='color: #666; font-size: 14px;'>Bu bağlantı 1 saat süreyle geçerlidir.</p>
                        <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 20px 0;'>
                        <p style='color: #666; font-size: 14px; font-style: italic;'>Eğer bu isteği siz yapmadıysanız, bu e-postayı görmezden gelebilirsiniz.</p>
                    </div>
                    <div style='text-align: center; padding: 20px; background-color: #f5f5f5; border-radius: 0 0 5px 5px;'>
                        <p style='color: #666; font-size: 12px; margin: 0;'>© 2024 Dental Health Tracker. Tüm hakları saklıdır.</p>
                    </div>
                </div>";

            return await SendEmailAsync(to, subject, body);
        }

        public async Task<bool> SendWelcomeEmailAsync(string to, string userName)
        {
            var subject = "Dental Health Tracker'a Hoş Geldiniz";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 5px;'>
                    <div style='background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0;'>
                        <h2 style='margin: 0;'>Hoş Geldiniz!</h2>
                    </div>
                    <div style='padding: 20px; background-color: #f9f9f9;'>
                        <p style='color: #333; font-size: 16px; line-height: 1.5;'>Merhaba {userName},</p>
                        <p style='color: #333; font-size: 16px; line-height: 1.5;'>Dental Health Tracker'a kaydınız başarıyla tamamlandı.</p>
                        <p style='color: #333; font-size: 16px; line-height: 1.5;'>Artık diş sağlığınızı takip etmeye başlayabilirsiniz.</p>
                    </div>
                    <div style='text-align: center; padding: 20px; background-color: #f5f5f5; border-radius: 0 0 5px 5px;'>
                        <p style='color: #666; font-size: 12px; margin: 0;'>© 2024 Dental Health Tracker. Tüm hakları saklıdır.</p>
                    </div>
                </div>";

            return await SendEmailAsync(to, subject, body);
        }
    }
}