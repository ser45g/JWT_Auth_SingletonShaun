namespace Webhooks.Processing.Webhooks
{
    public record class CreateWebhookRequest(string EventType, string WebhookUrl);
}
