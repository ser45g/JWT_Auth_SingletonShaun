namespace MyJwtAuthServer.Contracts.Events.BusinessEvents
{
    public record class UserEmailChangedEvent(string Username, string Email, string NewEmail, DateTime OccuredOnUtc);


}
