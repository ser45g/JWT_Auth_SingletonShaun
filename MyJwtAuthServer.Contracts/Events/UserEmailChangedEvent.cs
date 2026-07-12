
namespace MyJwtAuthServer.Contracts.Events
{
    public record class UserEmailChangedEvent(string Username, string Email, string NewEmail, DateTime OccuredOnUtc);


}
