using Microsoft.Extensions.Options;
using MyJwtAuthService.Options;
using MyJwtAuthService.Outbox;

namespace MyJwtAuthService.BackgroundServices
{
    public class OutboxBackgroundService(IServiceScopeFactory scopeFactory, IOptions<OutboxBackgroundServiceOptions> options) : BackgroundService
    {
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var intervalMiliseconds = options.Value.IntervalMiliseconds;
            var maxDegreeOfParallelism = options.Value.MaxDegreeOfParallelism;

            var parallelOptions = new ParallelOptions() { CancellationToken=stoppingToken, MaxDegreeOfParallelism = maxDegreeOfParallelism };

            try
            {
                await Parallel.ForEachAsync(Enumerable.Range(0, options.Value.MaxDegreeOfParallelism), parallelOptions, async (index, ct) =>
                {
                    await ProcessOutboxMessages(stoppingToken, intervalMiliseconds);
                });
            }
            catch(Exception ex)
            {
                var logger = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<OutboxBackgroundService>>();
                logger.LogError(ex, "An error occurred while processing outbox messages.");
            };
        }

        private async Task ProcessOutboxMessages(CancellationToken stoppingToken, int intervalMiliseconds) {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = scopeFactory.CreateScope();

                var outboxProcessor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();

                await outboxProcessor.ProcessOutboxMessagesAsync(stoppingToken);

                await Task.Delay(intervalMiliseconds, stoppingToken);
            }
        }
    }
}
