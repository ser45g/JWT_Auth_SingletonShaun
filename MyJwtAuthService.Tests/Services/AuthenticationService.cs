using Microsoft.AspNetCore.WebUtilities;
using MyJwtAuthService.Requests;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.Services
{
    public class AuthenticationService(IntegrationTestWebAppFactory factory, PapercutService papercutService)
    {
        public async Task<HttpResponseMessage> RegisterUser(RegisterRequest registerRequest)
        {
            ArgumentNullException.ThrowIfNull(registerRequest, nameof(registerRequest));

            using var client = factory.CreateDefaultClient();
            return await client.PostAsJsonAsync<RegisterRequest>("/auth/register", registerRequest);
        }
        public async Task<HttpResponseMessage> ConfirmEmailByFollowingLinkFromTheLastEmail()
        {
            using var mailServerHttpClient = new HttpClient() { BaseAddress = new Uri(factory.MailServerConnectionString) };

            var confirmationLink = await papercutService.GetConfirmationLinkFromLastEmailAsync();

            using var client = factory.CreateDefaultClient();

            return await client.GetAsync(confirmationLink);
        }

        public async Task<HttpResponseMessage> ResetPassword(ResetPasswordRequest resetPasswordRequest)
        {
            using var client = factory.CreateDefaultClient();

            return await client.PostAsJsonAsync<ResetPasswordRequest>("/auth/resetPassword", resetPasswordRequest);
        }

        public async Task<HttpResponseMessage> ConfirmEmail(string userId, string code, string? changedEmail=null)
        {
            using var client = factory.CreateDefaultClient();

            var requestUri = "/auth/confirmEmail";

            var query = new Dictionary<string, string?>{{ nameof(userId), userId },{ nameof(code), code }};

            if(changedEmail!=null)
                query.Add(nameof(changedEmail), changedEmail);

            var uri = QueryHelpers.AddQueryString(requestUri, query);

            return await client.GetAsync(uri);
        }

        public async Task<HttpResponseMessage> ConfirmEmail(string link)
        {
            using var client = factory.CreateDefaultClient();

            return await client.GetAsync(link);
        }

        public async Task<HttpResponseMessage> Login(LoginRequest loginRequest) {

            using var client = factory.CreateDefaultClient();

            return await client.PostAsJsonAsync<LoginRequest>("/auth/login", loginRequest);
        }

        public async Task<HttpResponseMessage> Refresh(RefreshRequest refreshRequest)
        {
            using var client = factory.CreateDefaultClient();

            return await client.PostAsJsonAsync<RefreshRequest>("/auth/refresh", refreshRequest);
        }

        public async Task<HttpResponseMessage> ResendConfirmationEmail(ResendRequest resendRequest)
        {
            using var client = factory.CreateDefaultClient();

            return await client.PostAsJsonAsync<ResendRequest>("/auth/resendConfirmationEmail", resendRequest);
        }

        public async Task<HttpResponseMessage> ForgotPassword(ForgotPasswordRequest forgotPasswordRequest)
        {
            using var client = factory.CreateDefaultClient();

            return await client.PostAsJsonAsync<ForgotPasswordRequest>("/auth/forgotPassword", forgotPasswordRequest);
        }

        public async Task<HttpResponseMessage> ChangeEmail(ChangeEmailRequest changeEmailRequest, string accessToken)
        {
            using var client = factory.CreateDefaultClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return await client.PostAsJsonAsync<ChangeEmailRequest>("/auth/changeEmail", changeEmailRequest);
        }

        public async Task<HttpResponseMessage> Logout(string accessToken)
        {
            using var client = factory.CreateDefaultClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return await client.DeleteAsync("/auth/logout");
        }

        public async Task<HttpResponseMessage> DeleteAccount(string accessToken)
        {
            using var client = factory.CreateDefaultClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return await client.DeleteAsync("/auth/delete-account");
        }

    }
}
