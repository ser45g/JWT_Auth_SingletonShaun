
namespace MyJwtAuthServer.Contracts.Events
{
    public record class WebhookDispatchedEvent(string EventType, object Payload, string? ParentActivityId);
}
