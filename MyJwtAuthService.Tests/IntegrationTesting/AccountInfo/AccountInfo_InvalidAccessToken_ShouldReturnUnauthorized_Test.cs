using Microsoft.AspNetCore.Mvc;
using MyJwtAuthService.Requests;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.AccountInfo
{
    public class AccountInfo_InvalidAccessToken_ShouldReturnUnauthorized_Test: BaseIntegrationTest
    {
        public AccountInfo_InvalidAccessToken_ShouldReturnUnauthorized_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task AccountInfo_InvalidAccessToken_ShouldReturnUnauthorized()
        {
            var registerRequest = new RegisterRequest { Email = "test@mail.com", Password = "Test2026*" };
            var loginRequest = new LoginRequest { Email = "test@mail.com", Password = "Test2026*" };

            var registerResponse = await authenticationService.RegisterUser(registerRequest);

            registerResponse.EnsureSuccessStatusCode();

            var confirmEmailLink = await papercutService.GetConfirmationLinkFromLastEmailAsync(isDelayed: true);
            WasNullException.ThrowIfNull(confirmEmailLink, nameof(confirmEmailLink));

            var confirmEmailResponse = await authenticationService.ConfirmEmail(confirmEmailLink);
            confirmEmailResponse.EnsureSuccessStatusCode();

            var logoutResponse = await authenticationService.GetAccountInfo("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.KMUFsIDTnFmyG3nMiGM6H9FNFUROf3wh7SmqJp-QV30");

            Assert.Equal(HttpStatusCode.Unauthorized, logoutResponse.StatusCode);

            ProblemDetails? problemDetails = await logoutResponse.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problemDetails == null)
                throw new Exception("Login should return problem details");

            Assert.NotNull(problemDetails.Extensions["timestamp"]);
            Assert.NotNull(problemDetails.Extensions["traceId"]);
        }
    }
}
