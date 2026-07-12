namespace MyJwtAuthServer.Contracts.Events.BusinessEvents
{
    public record class UserDeletedEvent(string Username, string Email, DateTime OccuredOnUtc);



}
