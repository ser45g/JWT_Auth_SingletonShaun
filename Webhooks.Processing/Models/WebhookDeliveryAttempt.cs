namespace Webhooks.Processing.Models
{
    public record class WebhookDeliveryAttempt(Guid Id, Guid WebhookSubscriptionId, int? ResponseStatusCode, bool IsSuccess, DateTime Timestamp);
}
