using Microsoft.EntityFrameworkCore;
using MyJwtAuthService.Requests;
using MyJwtAuthService.Responses;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.DeleteAccount
{
    public class DeleteAccount_ShouldDeleteUser_Test: BaseIntegrationTest
    {
        public DeleteAccount_ShouldDeleteUser_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task DeleteAccount_ShouldDeleteUser()
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

            if (authenticatedUserResponse?.AccessToken == null)
                throw new Exception();

            var deleteAccountResponse = await authenticationService.DeleteAccount(authenticatedUserResponse.AccessToken);

            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == registerRequest.Email);

            Assert.Null(user);
        }
    }
}
