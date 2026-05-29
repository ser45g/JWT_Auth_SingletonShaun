using Microsoft.EntityFrameworkCore;
using MyJwtAuthService.Requests;
using MyJwtAuthService.Responses;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.Logout
{
    public class Logout_ShouldInvalidateTokens_And_DeleteRefreshTokensForUserFromDb_Test : BaseIntegrationTest
    {
        public Logout_ShouldInvalidateTokens_And_DeleteRefreshTokensForUserFromDb_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task Logout_ShouldInvalidateTokens_And_DeleteRefreshTokensForUserFromDb()
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

            var logoutResponse = await authenticationService.Logout(authenticatedUserResponse.AccessToken);
            Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

            var amountOfRefreshTokens = await _dbContext.RefreshTokens.CountAsync();
            Assert.Equal(0, amountOfRefreshTokens);
        }
    }
}
