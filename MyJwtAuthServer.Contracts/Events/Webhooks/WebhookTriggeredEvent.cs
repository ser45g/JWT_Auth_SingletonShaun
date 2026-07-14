namespace MyJwtAuthServer.Contracts.Events.Webhooks
{
    public record class WebhookTriggeredEvent(Guid SubscriptionId, string EventType, string WebhookUrl, object Data);

}
