namespace Webhooks.Processing.Models
{
    public record class WebhookSubscription(Guid Id, string EventType, string WebhookUrl, DateTime CreatedOnUtc);
}
