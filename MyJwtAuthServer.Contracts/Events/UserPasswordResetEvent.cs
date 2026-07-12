namespace MyJwtAuthServer.Contracts.Events
{
    public record class UserPasswordResetEvent(string Username, string Email, string ResetCode, DateTime OccuredOnUtc);
}
