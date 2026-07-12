
namespace MyJwtAuthServer.Contracts.Events
{
    public record class UserEmailConfirmedEvent(string Username, string Email, DateTime OccuredOnUtc);


}
