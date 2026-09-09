namespace MyJwtAuthService.Outbox
{
    public partial class OutboxProcessor
    {
        public struct OutboxUpdate
        {
            public Guid Id { get; init; }
            public DateTime ProcessedOnUtc { get; init; }
            public string? Error { get; init; }
        }
        
    }
}
