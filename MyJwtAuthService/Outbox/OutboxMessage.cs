namespace MyJwtAuthService.Outbox
{
    public record class OutboxMessage(Guid Id, string Type, string Content, DateTime OccuredOnUtc, DateTime? ProcessedOnUtc = null, string? Error = null);
}
