namespace MyJwtAuthServer.Contracts.Events
{
    public record class RegistrationEmailConfirmationSentEvent(string Email, string ConfirmationLink);

}
