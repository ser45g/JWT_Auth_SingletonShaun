namespace MyJwtAuthServer.Contracts.Events.BusinessEvents
{
    public record class UserLoggedOutEvent(string Username, string Email, DateTime OccuredOnUtc);


}
