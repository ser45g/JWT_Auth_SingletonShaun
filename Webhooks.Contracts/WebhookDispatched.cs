namespace Webhooks.Processing
{
    public record class WebhookDispatched(string EventType, object Payload, string? ParentActivityId);
}
