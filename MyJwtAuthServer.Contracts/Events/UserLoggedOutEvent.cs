
namespace MyJwtAuthServer.Contracts.Events
{
    public record class UserLoggedOutEvent(string Username, string Email, DateTime OccuredOnUtc);


}
