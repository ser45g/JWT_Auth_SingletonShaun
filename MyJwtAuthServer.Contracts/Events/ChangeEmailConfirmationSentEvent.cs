
namespace MyJwtAuthServer.Contracts.Events
{
    public record class ChangeEmailConfirmationSentEvent(string Email, string ConfirmationLink);

}
