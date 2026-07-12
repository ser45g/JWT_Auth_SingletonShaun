
namespace MyJwtAuthServer.Contracts.Events
{
    public record class UserLoggedInEvent(string Username, string Email, DateTime OccuredOnUtc);


}
