namespace Webhooks.Processing
{
    public record class WebhookSubscriptionAdded(Guid Id, string EventType, string WebhookUrl);

}
