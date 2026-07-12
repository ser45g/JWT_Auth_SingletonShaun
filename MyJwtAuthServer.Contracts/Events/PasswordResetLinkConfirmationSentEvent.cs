namespace MyJwtAuthServer.Contracts.Events
{
    public record class PasswordResetLinkConfirmationSentEvent(string Email, string ResetLink);

}
