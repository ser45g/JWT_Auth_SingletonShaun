using Microsoft.AspNetCore.Mvc;
using MyJwtAuthService.Requests;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.Logout
{
    public class Logout_InvalidAccessToken_ShouldReturnUnauthorized_Test: BaseIntegrationTest
    {
        public Logout_InvalidAccessToken_ShouldReturnUnauthorized_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task Logout_InvalidAccessToken_ShouldReturnUnauthorized()
        {
            var registerRequest = new RegisterRequest { Email = "test@mail.com", Password = "Test2026*" };
            var loginRequest = new LoginRequest { Email = "test@mail.com", Password = "Test2026*" };

            var registerResponse = await authenticationService.RegisterUser(registerRequest);

            registerResponse.EnsureSuccessStatusCode();

            var confirmEmailResponse = await authenticationService.ConfirmEmailByFollowingLinkFromTheLastEmail();

            confirmEmailResponse.EnsureSuccessStatusCode();

            var logoutResponse = await authenticationService.Logout("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.KMUFsIDTnFmyG3nMiGM6H9FNFUROf3wh7SmqJp-QV30");

            Assert.Equal(HttpStatusCode.Unauthorized, logoutResponse.StatusCode);

            ProblemDetails? problemDetails = await logoutResponse.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problemDetails == null)
                throw new Exception("Login should return problem details");

            Assert.NotNull(problemDetails.Extensions["timestamp"]);
            Assert.NotNull(problemDetails.Extensions["traceId"]);
        }
    }
}
