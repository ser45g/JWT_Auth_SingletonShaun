namespace MyJwtAuthService.Requests
{
    public record class CreateWebhookRequest(string EventType, string WebhookUrl);
}
