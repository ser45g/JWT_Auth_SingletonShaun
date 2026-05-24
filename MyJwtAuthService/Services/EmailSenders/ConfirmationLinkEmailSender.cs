using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using MyJwtAuthService.Models;
using System.Text;
using System.Text.Encodings.Web;

namespace MyJwtAuthService.Services.EmailSenders
{
    public class ConfirmationLinkEmailSender : IConfirmationLinkEmailSender
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly LinkGenerator linkGenerator;
        private readonly IEmailSender<ApplicationUser> emailSender;

        public ConfirmationLinkEmailSender(UserManager<ApplicationUser> userManager, LinkGenerator linkGenerator, IEmailSender<ApplicationUser> emailSender)
        {
            this.userManager = userManager;
            this.linkGenerator = linkGenerator;
            this.emailSender = emailSender;
        }

        public async Task SendConfirmationEmailAsync(ApplicationUser user, string email, HttpContext context, string confirmEmailEndpointName, bool isEmailChanged = false)
        {
            if (confirmEmailEndpointName == null)
            {
                throw new NotSupportedException("No email confirmation endpoint was registered!");
            }

            string text = (!isEmailChanged) ? (await userManager.GenerateEmailConfirmationTokenAsync(user)) : (await userManager.GenerateChangeEmailTokenAsync(user, email));

            string code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(text));

            string value = await userManager.GetUserIdAsync(user);

            RouteValueDictionary routeValueDictionary = new RouteValueDictionary
            {
                ["userId"] = value,
                ["code"] = code
            };
            if (isEmailChanged)
            {
                routeValueDictionary.Add("changedEmail", email);
            }

            string link = linkGenerator.GetUriByName(context, confirmEmailEndpointName, routeValueDictionary) ?? throw new NotSupportedException("Could not find endpoint named '" + confirmEmailEndpointName + "'.");

            await emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(link));
        }
    }
}
