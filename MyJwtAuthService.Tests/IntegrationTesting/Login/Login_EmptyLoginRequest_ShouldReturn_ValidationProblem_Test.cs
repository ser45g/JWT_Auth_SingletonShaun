using Microsoft.AspNetCore.Mvc;
using MyJwtAuthService.Requests;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.Login
{
    public class Login_EmptyLoginRequest_ShouldReturn_ValidationProblem_Test : BaseIntegrationTest
    {

        public Login_EmptyLoginRequest_ShouldReturn_ValidationProblem_Test(IntegrationTestWebAppFactory factory) : base(factory)
        {

        }

        [Fact]
        public async Task Login_EmptyLoginRequest_ShouldReturn_ValidationProblem()
        {
            var registerRequest = new RegisterRequest { Email = "test@mail.com", Password = "Test2026*" };
            var loginRequest = new LoginRequest { Email = "", Password = "" };

            var registerResponse = await authenticationService.RegisterUser(registerRequest);

            registerResponse.EnsureSuccessStatusCode();

            var confirmEmailResponse = await authenticationService.ConfirmEmailByFollowingLinkFromTheLastEmail();

            confirmEmailResponse.EnsureSuccessStatusCode();

            var loginResponse = await authenticationService.Login(loginRequest);

            Assert.Equal(HttpStatusCode.BadRequest, loginResponse.StatusCode);

            ProblemDetails? problemDetails = await loginResponse.Content.ReadFromJsonAsync<ProblemDetails>();
            if (problemDetails == null)
                throw new Exception(nameof(problemDetails));

            Assert.NotNull(problemDetails.Extensions["errors"]);
            Assert.NotNull(problemDetails.Extensions["traceId"]);
            Assert.NotNull(problemDetails.Extensions["timestamp"]);
        }

    }
}
