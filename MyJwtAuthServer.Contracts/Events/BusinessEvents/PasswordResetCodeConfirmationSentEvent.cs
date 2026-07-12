namespace MyJwtAuthServer.Contracts.Events.BusinessEvents
{
    public record class PasswordResetCodeConfirmationSentEvent(string Email, string ResetCode);
}
