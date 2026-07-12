using MassTransit;
using MyJwtAuthServer.Contracts.Events;
using MyJwtAuthService.Services.EmailSenders;

namespace MyJwtAuthService.Consumers
{
    public class PasswordResetCodeConfirmationConsumer(IEmailService emailService) : IConsumer<PasswordResetCodeConfirmationSentEvent>
    {
        public async Task Consume(ConsumeContext<PasswordResetCodeConfirmationSentEvent> context)
        {
            var notification = context.Message;

            await emailService.SendEmailAsync(notification.Email, "Password Reset Code Confirmation", $"Your password reset code is: {notification.ResetCode}", context.CancellationToken);

        }
    }
}
