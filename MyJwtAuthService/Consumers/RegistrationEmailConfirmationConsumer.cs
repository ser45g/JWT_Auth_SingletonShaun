using MassTransit;
using MyJwtAuthServer.Contracts.Events;
using MyJwtAuthService.Services.EmailSenders;

namespace MyJwtAuthService.Consumers
{
    public class RegistrationEmailConfirmationConsumer(IEmailService emailService) : IConsumer<RegistrationEmailConfirmationSentEvent>
    {
        public async Task Consume(ConsumeContext<RegistrationEmailConfirmationSentEvent> context)
        {
            var notification = context.Message;

            await emailService.SendEmailAsync(notification.Email, "Registration Email Confirmation", $"Your registration confirmation link is: {notification.ConfirmationLink}", context.CancellationToken);

        }
    }
}
