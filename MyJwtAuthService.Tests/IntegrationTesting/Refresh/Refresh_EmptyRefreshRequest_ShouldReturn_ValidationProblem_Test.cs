using Microsoft.AspNetCore.Mvc;
using MyJwtAuthService.Requests;
using MyJwtAuthService.Responses;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.Refresh
{
    public class Refresh_EmptyRefreshRequest_ShouldReturn_ValidationProblem_Test : BaseIntegrationTest
    {

        public Refresh_EmptyRefreshRequest_ShouldReturn_ValidationProblem_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task Refresh_EmptyRefreshRequest_ShouldReturn_ValidationProblem()
        {
            var client = _factory.CreateDefaultClient();

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
            var refreshRequest = new RefreshRequest() { RefreshToken = "" };

            var refreshResponse = await client.PostAsJsonAsync<RefreshRequest>("/auth/refresh", refreshRequest);

            Assert.Equal(HttpStatusCode.BadRequest, refreshResponse.StatusCode);

            ProblemDetails? problemDetails = await refreshResponse.Content.ReadFromJsonAsync<ProblemDetails>();
            if (problemDetails == null)
                throw new Exception(nameof(problemDetails));

            Assert.NotNull(problemDetails.Extensions["errors"]);
            Assert.NotNull(problemDetails.Extensions["traceId"]);
            Assert.NotNull(problemDetails.Extensions["timestamp"]);
        }
    }
}

