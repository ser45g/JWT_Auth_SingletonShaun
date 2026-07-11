namespace Webhooks.Processing
{
    public record class WebhookPayload<T>(Guid Id, string EventType, Guid SubscriptionId, DateTime Timestamp, T Data);
}
