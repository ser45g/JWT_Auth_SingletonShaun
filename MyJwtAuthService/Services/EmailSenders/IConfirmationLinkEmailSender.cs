using MyJwtAuthService.Models;

namespace MyJwtAuthService.Services.EmailSenders
{
    public interface IConfirmationLinkEmailSender
    {
        Task SendConfirmationEmailAsync(ApplicationUser user, string email, HttpContext context, string confirmEmailEndpointName, bool isChange = false);
    }
}