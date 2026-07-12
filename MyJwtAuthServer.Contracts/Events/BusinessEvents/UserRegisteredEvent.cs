namespace MyJwtAuthServer.Contracts.Events.BusinessEvents
{
    public record class UserRegisteredEvent(string Username, string Email, DateTime TimestampUtc);
    
}
