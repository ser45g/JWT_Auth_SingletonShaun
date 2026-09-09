using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using MyJwtAuthService.Models;
using System.Text;

namespace MyJwtAuthService.Services.EmailSenders
{
    public class ApplicationLinkGenerator : IApplicationLinkGenerator
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly LinkGenerator linkGenerator;
        private readonly IEmailSender<ApplicationUser> emailSender;

        public ApplicationLinkGenerator(UserManager<ApplicationUser> userManager, LinkGenerator linkGenerator, IEmailSender<ApplicationUser> emailSender)
        {
            this.userManager = userManager;
            this.linkGenerator = linkGenerator;
            this.emailSender = emailSender;
        }

        public async Task<string?> GetEmailConfirmationLink(ApplicationUser user, string email, HttpContext context, string confirmEmailEndpointName, bool isEmailChanged = false)
        {
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

            var link = linkGenerator.GetUriByName(context, confirmEmailEndpointName, routeValueDictionary); //?? throw new NotSupportedException("Could not find endpoint named '" + confirmEmailEndpointName + "'.");

            return link;
            //await emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(link));
        }
    }
}
