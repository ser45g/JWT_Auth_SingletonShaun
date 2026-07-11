namespace Webhooks.Processing
{
    public record class WebhookTriggered(Guid SubscriptionId, string EventType, string WebhookUrl, object Data);

}
