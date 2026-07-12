namespace MyJwtAuthServer.Contracts.Events.BusinessEvents
{
    public record class UserEmailConfirmedEvent(string Username, string Email, DateTime OccuredOnUtc);


}
