using MyJwtAuthService.Requests;
using MyJwtAuthService.Responses;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.ChangeEmail
{
    public class ChangeEmail_ShouldMailEmailChangeToken_Test : BaseIntegrationTest
    {
        public ChangeEmail_ShouldMailEmailChangeToken_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task ChangeEmail_ShouldMailEmailChangeToken()
        {
            var registerRequest = new RegisterRequest { Email = "test@mail.com", Password = "Test2026*" };
            var loginRequest = new LoginRequest { Email = "test@mail.com", Password = "Test2026*" };

            var registerResponse = await authenticationService.RegisterUser(registerRequest);

            registerResponse.EnsureSuccessStatusCode();

            var confirmEmailLink = await papercutService.GetConfirmationLinkFromLastEmailAsync(isDelayed: true);
            WasNullException.ThrowIfNull(confirmEmailLink, nameof(confirmEmailLink));

            var confirmEmailResponse = await authenticationService.ConfirmEmail(confirmEmailLink);
            confirmEmailResponse.EnsureSuccessStatusCode();

            var loginResponse = await authenticationService.Login(loginRequest);

            loginResponse.EnsureSuccessStatusCode();

            var loginAuthenticatedUserResponse = await loginResponse.Content.ReadFromJsonAsync<AuthenticatedUserResponse>();

            if (loginAuthenticatedUserResponse?.AccessToken==null)
                throw new Exception();

            var changeEmailRequest = new ChangeEmailRequest() { NewEmail="newtest@mail.com" };

            var changeEmailResponse = await authenticationService.ChangeEmail(changeEmailRequest, loginAuthenticatedUserResponse.AccessToken);

            changeEmailResponse.EnsureSuccessStatusCode();

            var confirmChangeEmailLink= await papercutService.GetConfirmationLinkFromLastEmailAsync(isDelayed: true);
            WasNullException.ThrowIfNull(confirmEmailLink, nameof(confirmEmailLink));

            var confirmChangeEmailResponse = await authenticationService.ConfirmEmail(confirmChangeEmailLink);
            confirmEmailResponse.EnsureSuccessStatusCode();

            var loginWithNewEmailRequest = new LoginRequest() { Email = changeEmailRequest.NewEmail, Password = registerRequest.Password };

            var loginWithNewEmailResponse = await authenticationService.Login(loginWithNewEmailRequest);

            Assert.Equal(HttpStatusCode.OK, loginWithNewEmailResponse.StatusCode);

            var loginWithNewEmailAuthenticatedUserResponse = await loginWithNewEmailResponse.Content.ReadFromJsonAsync<AuthenticatedUserResponse>();

            Assert.NotNull(loginWithNewEmailAuthenticatedUserResponse);
            Assert.NotEmpty(loginWithNewEmailAuthenticatedUserResponse.AccessToken);
            Assert.NotEqual(DateTime.MinValue, loginWithNewEmailAuthenticatedUserResponse.AccessTokenExpirationTime);
            Assert.NotEmpty(loginWithNewEmailAuthenticatedUserResponse.RefreshToken);
        }
    }
}
