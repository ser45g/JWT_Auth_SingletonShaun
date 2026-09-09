using MediatR;
using Microsoft.AspNetCore.Identity;
using MyJwtAuthService.Models;
using MyJwtAuthService.Outbox.Messages;

namespace MyJwtAuthService.Outbox.Handlers
{
    public class RegistrationEmailConfirmationHandler(IEmailSender<ApplicationUser> emailSender) : INotificationHandler<RegistrationEmailConfirmationOutboxMessage>
    {
        public async Task Handle(RegistrationEmailConfirmationOutboxMessage notification, CancellationToken cancellationToken)
        {
            await emailSender.SendConfirmationLinkAsync(notification.User, notification.Email, notification.ConfirmationLink);
        }
    }
}
