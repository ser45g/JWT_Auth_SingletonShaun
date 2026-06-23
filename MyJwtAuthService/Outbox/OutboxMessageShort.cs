namespace MyJwtAuthService.Outbox
{
    public partial class OutboxProcessor
    {
        public record class OutboxMessageShort(Guid Id, string Type, string Content);
        
    }
}
