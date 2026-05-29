using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyJwtAuthService.Requests;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.DeleteAccount
{
    public class DeleteAccount_InvalidAccessToken_ShouldReturnUnauthorized_Test: BaseIntegrationTest
    {
        public DeleteAccount_InvalidAccessToken_ShouldReturnUnauthorized_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task DeleteAccount_InvalidAccessToken_ShouldReturnUnauthorized()
        {
            var registerRequest = new RegisterRequest { Email = "test@mail.com", Password = "Test2026*" };

            var registerResponse = await authenticationService.RegisterUser(registerRequest);

            registerResponse.EnsureSuccessStatusCode();

            var confirmEmailResponse = await authenticationService.ConfirmEmailByFollowingLinkFromTheLastEmail();

            confirmEmailResponse.EnsureSuccessStatusCode();

            var deleteAccountResponse = await authenticationService.DeleteAccount("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.KMUFsIDTnFmyG3nMiGM6H9FNFUROf3wh7SmqJp-QV30");

            Assert.Equal(HttpStatusCode.Unauthorized, deleteAccountResponse.StatusCode);

            ProblemDetails? problemDetails = await deleteAccountResponse.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problemDetails == null)
                throw new Exception("Should return problem details");

            Assert.NotNull(problemDetails.Extensions["timestamp"]);
            Assert.NotNull(problemDetails.Extensions["traceId"]);

            var user = _dbContext.Users.FirstOrDefaultAsync(x => x.Email == registerRequest.Email);

            Assert.NotNull(user);
        }
    }
}
