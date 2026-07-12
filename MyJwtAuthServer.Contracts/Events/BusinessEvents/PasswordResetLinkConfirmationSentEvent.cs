namespace MyJwtAuthServer.Contracts.Events.BusinessEvents
{
    public record class PasswordResetLinkConfirmationSentEvent(string Email, string ResetLink);

}
