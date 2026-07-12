namespace MyJwtAuthServer.Contracts.Events
{
    public record class WebhookSubscriptionAddedEvent(Guid Id, string EventType, string WebhookUrl);

}
