using MyJwtAuthService.Models;

namespace MyJwtAuthService.Services.EmailSenders
{
    public interface IApplicationLinkGenerator
    {
        Task<string?> GetEmailConfirmationLink(ApplicationUser user, string email, HttpContext context, string confirmEmailEndpointName, bool isEmailChanged = false);
    }
}