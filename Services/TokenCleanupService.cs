using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DentalHealthTracker.Services
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1);

        public TokenCleanupService(
            IServiceProvider serviceProvider,
            ILogger<TokenCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var passwordResetService = scope.ServiceProvider.GetRequiredService<IPasswordResetService>();
                    await passwordResetService.CleanupExpiredTokensAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Token temizleme işlemi sırasında hata oluştu.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}