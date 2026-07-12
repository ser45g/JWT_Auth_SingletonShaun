
namespace MyJwtAuthServer.Contracts.Events
{
    public record class UserRegisteredEvent(string Username, string Email, DateTime TimestampUtc);


    
}
