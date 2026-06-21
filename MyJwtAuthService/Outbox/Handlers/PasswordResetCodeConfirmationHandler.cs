using MediatR;
using Microsoft.AspNetCore.Identity;
using MyJwtAuthService.Models;
using MyJwtAuthService.Outbox.Messages;

namespace MyJwtAuthService.Outbox.Handlers
{
    public class PasswordResetCodeConfirmationHandler(IEmailSender<ApplicationUser> emailSender) : INotificationHandler<PasswordResetCodeConfirmationOutboxMessage>
    {
        public async Task Handle(PasswordResetCodeConfirmationOutboxMessage notification, CancellationToken cancellationToken)
        {
            await emailSender.SendConfirmationLinkAsync(notification.User, notification.Email, notification.ResetCode);
        }
    }
}
