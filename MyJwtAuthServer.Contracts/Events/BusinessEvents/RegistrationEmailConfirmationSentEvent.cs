namespace MyJwtAuthServer.Contracts.Events.BusinessEvents
{
    public record class RegistrationEmailConfirmationSentEvent(string Email, string ConfirmationLink);

}
