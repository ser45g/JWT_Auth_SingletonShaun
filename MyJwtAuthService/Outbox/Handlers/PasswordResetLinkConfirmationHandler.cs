using MediatR;
using Microsoft.AspNetCore.Identity;
using MyJwtAuthService.Models;
using MyJwtAuthService.Outbox.Messages;

namespace MyJwtAuthService.Outbox.Handlers
{
    public class PasswordResetLinkConfirmationHandler(IEmailSender<ApplicationUser> emailSender) : INotificationHandler<PasswordResetLinkConfirmationOutboxMessage>
    {
        public async Task Handle(PasswordResetLinkConfirmationOutboxMessage notification, CancellationToken cancellationToken)
        {
            await emailSender.SendConfirmationLinkAsync(notification.User, notification.Email, notification.ResetLink);
        }
    }
}
