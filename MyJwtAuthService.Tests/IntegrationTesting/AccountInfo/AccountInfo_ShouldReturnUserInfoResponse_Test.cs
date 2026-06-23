using Microsoft.EntityFrameworkCore;
using MyJwtAuthService.Requests;
using MyJwtAuthService.Responses;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.AccountInfo
{
    public class AccountInfo_ShouldReturnUserInfoResponse_Test : BaseIntegrationTest
    {
        public AccountInfo_ShouldReturnUserInfoResponse_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task AccountInfo_ShouldReturnUserInfoResponse()
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

            var authenticatedUserResponse = await loginResponse.Content.ReadFromJsonAsync<AuthenticatedUserResponse>();

            Assert.NotNull(authenticatedUserResponse);
            Assert.NotEmpty(authenticatedUserResponse.AccessToken);
            Assert.NotEqual(DateTime.MinValue, authenticatedUserResponse.AccessTokenExpirationTime);
            Assert.NotEmpty(authenticatedUserResponse.RefreshToken);

            var accountInfoResponse = await authenticationService.GetAccountInfo(authenticatedUserResponse.AccessToken);
            Assert.Equal(HttpStatusCode.OK, accountInfoResponse.StatusCode);

            var account = await accountInfoResponse.Content.ReadFromJsonAsync<UserInfoResponse>();
            Assert.NotNull(account);

            Assert.Equal(registerRequest.Email, account.Email);
            Assert.Equal(registerRequest.Email, account.Username);
            Assert.Equal(true, account?.EmailConfirmed);
            Assert.True(account?.Id != Guid.Empty);
        }
    }
}
