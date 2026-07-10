namespace MyJwtAuthService.Webhooks
{
    public record class WebhookDispatch(string EventType, object Payload, string? ParentActivityId);
}
