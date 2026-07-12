namespace MyJwtAuthServer.Contracts.Events.BusinessEvents
{
    public record class UserLoggedInEvent(string Username, string Email, DateTime OccuredOnUtc);


}
