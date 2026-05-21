using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MyJwtAuthService.Services.EmailSenders
{
    public class EmailService(IOptions<MailSettings> _mailSettingsOptions) : IEmailService
    {
        public async Task SendEmailAsync(string email, string subject, string message, CancellationToken cancellationToken = default)
        {
            var mailSettings = _mailSettingsOptions.Value;
            using var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(mailSettings.DisplayName, mailSettings.From));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(mailSettings.Host, mailSettings.Port, mailSettings.IsAuthenticated, cancellationToken);

                if (mailSettings.IsAuthenticated)
                    await client.AuthenticateAsync(mailSettings.UserName, mailSettings.Password, cancellationToken);

                await client.SendAsync(emailMessage, cancellationToken);

                await client.DisconnectAsync(true, cancellationToken);
            }
        }
    }
}
