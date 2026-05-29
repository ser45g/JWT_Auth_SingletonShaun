using MyJwtAuthService.Requests;
using MyJwtAuthService.Responses;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.Login
{
    public class Login_ShouldReturn_AuthenticatedUserResponse_Test : BaseIntegrationTest
    {

        public Login_ShouldReturn_AuthenticatedUserResponse_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task Login_ShouldReturn_AuthenticatedUserResponse()
        {
            var registerRequest = new RegisterRequest { Email = "test@mail.com", Password = "Test2026*" };
            var loginRequest = new LoginRequest { Email = "test@mail.com", Password = "Test2026*" };

            var registerResponse = await authenticationService.RegisterUser(registerRequest);

            registerResponse.EnsureSuccessStatusCode();

            var confirmEmailResponse = await authenticationService.ConfirmEmailByFollowingLinkFromTheLastEmail();

            confirmEmailResponse.EnsureSuccessStatusCode();

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
