namespace MyJwtAuthService.Outbox
{
    public record class OutboxMessage(Guid Id, string Type, string Content, DateTime OccuredOnUtc, int RetryAttemptsCount=0, bool IsUnprocessable=false, DateTime? ProcessedOnUtc = null, string? Error = null);
}
