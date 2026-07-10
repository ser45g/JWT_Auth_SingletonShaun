using MyJwtAuthService.Helpers;
using System.Threading.Channels;

namespace MyJwtAuthService.Webhooks
{
    public class WebhookProcessor : BackgroundService
    {
        private IServiceScopeFactory serviceScopeFactory;
        private readonly Channel<WebhookDispatch> channel;

        public WebhookProcessor(IServiceScopeFactory serviceScopeFactory, Channel<WebhookDispatch> channel)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.channel = channel;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var dispatch in channel.Reader.ReadAllAsync(stoppingToken))
            {
                using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{dispatch.EventType} processing webhook", System.Diagnostics.ActivityKind.Internal, dispatch.ParentActivityId);


                using var scope = serviceScopeFactory.CreateScope();
                var webhookDispatcher = scope.ServiceProvider.GetRequiredService<WebhookDispatcher>();
                await webhookDispatcher.ProcessAsync(dispatch.EventType, dispatch.Payload, stoppingToken);
            }
        }
    }
}
