namespace MyJwtAuthServer.Contracts.Events.Webhooks
{
    public record class WebhookDispatchedEvent(string EventType, object Payload, string? ParentActivityId);
}
