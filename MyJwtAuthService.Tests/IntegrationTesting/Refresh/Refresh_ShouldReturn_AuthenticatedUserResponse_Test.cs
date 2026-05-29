using MyJwtAuthService.Requests;
using MyJwtAuthService.Responses;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.Refresh
{
    public class Refresh_ShouldReturn_AuthenticatedUserResponse_Test : BaseIntegrationTest
    {

        public Refresh_ShouldReturn_AuthenticatedUserResponse_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task Refresh_ShouldReturn_AuthenticatedUserResponse()
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

            if (authenticatedUserResponse?.RefreshToken == null)
            {
                throw new Exception(nameof(authenticatedUserResponse));
            }
            var refreshRequest = new RefreshRequest() { RefreshToken = authenticatedUserResponse.RefreshToken };

            var refreshResponse = await authenticationService.Refresh(refreshRequest);

            var refreshAuthenticatedUserResponse = await refreshResponse.Content.ReadFromJsonAsync<AuthenticatedUserResponse>();

            Assert.NotNull(refreshAuthenticatedUserResponse);

            Assert.NotEmpty(refreshAuthenticatedUserResponse.AccessToken);
            Assert.NotEqual(DateTime.MinValue, refreshAuthenticatedUserResponse.AccessTokenExpirationTime);
            Assert.NotEmpty(refreshAuthenticatedUserResponse.RefreshToken);
        }
    }
}
