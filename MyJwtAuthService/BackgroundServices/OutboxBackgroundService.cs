using Microsoft.Extensions.Options;
using MyJwtAuthService.Models;
using MyJwtAuthService.Outbox;

namespace MyJwtAuthService.BackgroundServices
{
    public class OutboxBackgroundService(IServiceScopeFactory scopeFactory, IOptions<OutboxBackgroundServiceConfiguration> options) : BackgroundService
    {
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var intervalSeconds = options.Value.IntervalSeconds;
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = scopeFactory.CreateScope();

                    var outboxProcessor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();

                    await outboxProcessor.ProcessOutboxMessagesAsync(stoppingToken);

                    await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
                }
            }
            catch(Exception ex)
            {
                var logger = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<OutboxBackgroundService>>();
                logger.LogError(ex, "An error occurred while processing outbox messages.");
            };
        }
    }
}
