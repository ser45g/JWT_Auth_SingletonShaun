
namespace MyJwtAuthServer.Contracts.Events
{
    public record class UserDeletedEvent(string Username, string Email, DateTime OccuredOnUtc);



}
