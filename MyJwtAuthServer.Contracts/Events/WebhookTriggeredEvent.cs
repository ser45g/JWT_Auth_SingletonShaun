
namespace MyJwtAuthServer.Contracts.Events
{
    public record class WebhookTriggeredEvent(Guid SubscriptionId, string EventType, string WebhookUrl, object Data);

}
