
namespace MyJwtAuthServer.Contracts.Events
{
    public record class PasswordResetCodeConfirmationSentEvent(string Email, string ResetCode);
}
