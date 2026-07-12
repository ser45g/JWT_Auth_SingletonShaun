using MassTransit;
using MyJwtAuthServer.Contracts.Events;
using MyJwtAuthService.Helpers;

namespace MyJwtAuthService.Webhooks
{
    public class WebhookDispatcher(IPublishEndpoint publishEndpoint)
    {
        public async Task DispatchAsync<T>(string eventType, T payload, CancellationToken cancellationToken = default) where T : notnull
        {
            using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{eventType} dispatched webhook");

            activity?.AddTag("event.type", eventType);

            await publishEndpoint.Publish(new WebhookDispatchedEvent(eventType, payload, activity?.Id), cancellationToken);
        }

        public async Task AddSubscription(string eventType, string webhookUrl, CancellationToken cancellationToken = default)
        {
            using var activity = DiagnosticConfig.ActivitySource.StartActivity($"{eventType} subscription added");

            activity?.AddTag("event.type", eventType);

            await publishEndpoint.Publish(new WebhookSubscriptionAddedEvent(Guid.NewGuid(), eventType, webhookUrl), cancellationToken);
        }
    }
}
