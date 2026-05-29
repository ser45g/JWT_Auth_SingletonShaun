using MyJwtAuthService.Requests;
using MyJwtAuthService.Responses;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.ConfirmEmail
{
    public class ConfirmEmail_ShouldConfirmRegistration_Test : BaseIntegrationTest
    {
        public ConfirmEmail_ShouldConfirmRegistration_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task ConfirmEmail_ShouldConfirmRegistration()
        {
            var registerRequest = new RegisterRequest { Email = "test@mail.com", Password = "Test2026*" };
            var loginRequest = new LoginRequest { Email = "test@mail.com", Password = "Test2026*" };

            var registerResponse = await authenticationService.RegisterUser(registerRequest);

            registerResponse.EnsureSuccessStatusCode();

            var unsuccessfulLoginResponse = await authenticationService.Login(loginRequest);

            Assert.False(unsuccessfulLoginResponse.IsSuccessStatusCode);

            var link = await papercutService.GetConfirmationLinkFromLastEmailAsync();

            if (link == null)
                throw new Exception();

            var confirmationLinkResponse = await authenticationService.ConfirmEmail(link);

            confirmationLinkResponse.EnsureSuccessStatusCode();

            var loginResponse = await authenticationService.Login(loginRequest);

            loginResponse.EnsureSuccessStatusCode();

            var authenticatedUserResponse = await loginResponse.Content.ReadFromJsonAsync<AuthenticatedUserResponse>();

            Assert.NotNull(authenticatedUserResponse);
            Assert.NotEmpty(authenticatedUserResponse.AccessToken);
            Assert.NotEqual(DateTime.MinValue, authenticatedUserResponse.AccessTokenExpirationTime);
            Assert.NotEmpty(authenticatedUserResponse.RefreshToken);

        }
    }
}
