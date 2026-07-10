namespace MyJwtAuthService.Webhooks
{
    public record class WebhookSubscription(Guid Id, string EventType, string WebhookUrl, DateTime CreatedOnUtc);
}
