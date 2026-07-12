using MassTransit;
using MyJwtAuthServer.Contracts.Events;
using MyJwtAuthService.Services.EmailSenders;

namespace MyJwtAuthService.Consumers
{
    public class PasswordResetLinkConfirmationConsumer(IEmailService emailService) : IConsumer<PasswordResetLinkConfirmationSentEvent>
    {
        public async Task Consume(ConsumeContext<PasswordResetLinkConfirmationSentEvent> context)
        {
            var notification = context.Message;

            await emailService.SendEmailAsync(notification.Email, "Password Reset Link Confirmation", $"Your password reset link is: {notification.ResetLink}", context.CancellationToken);

        }
    }
}
