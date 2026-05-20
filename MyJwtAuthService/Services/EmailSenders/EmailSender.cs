using Microsoft.AspNetCore.Identity;
using MyJwtAuthService.Models;

namespace MyJwtAuthService.Services.EmailSenders
{
    public class EmailSender : IEmailSender<ApplicationUser>
    {
        private readonly ILogger _logger;
        private readonly IEmailService _emailService;

        public EmailSender(ILogger<EmailSender> logger, IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            await _emailService.SendEmailAsync(email, "Please confirm your email", $"Please comfirm your email here: {confirmationLink}");
        }

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            await _emailService.SendEmailAsync(email, "Reset code", $"Here is your reset code: {resetCode}");
        }

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            await _emailService.SendEmailAsync(email, "Password reset link", $"Please comfirm your email here: {resetLink}");
        }
    }
}
