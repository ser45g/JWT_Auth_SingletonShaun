namespace MyJwtAuthServer.Contracts.Events.BusinessEvents
{
    public record class UserPasswordResetEvent(string Username, string Email, string ResetCode, DateTime OccuredOnUtc);
}
