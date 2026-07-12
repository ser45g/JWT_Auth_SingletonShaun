namespace MyJwtAuthServer.Contracts.Events.BusinessEvents
{
    public record class ChangeEmailConfirmationSentEvent(string Email, string ConfirmationLink);

}
