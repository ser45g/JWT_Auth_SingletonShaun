using Microsoft.AspNetCore.Mvc;
using MyJwtAuthService.Requests;
using MyJwtAuthService.Responses;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.ChangeEmail
{
    public class ChangeEmail_EmptyChangeEmailRequest_ShouldReturnValidationProblems_Test : BaseIntegrationTest
    {
        public ChangeEmail_EmptyChangeEmailRequest_ShouldReturnValidationProblems_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task ChangeEmail_EmptyChangeEmailRequest_ShouldReturnValidationProblems()
        {
            var registerRequest = new RegisterRequest { Email = "test@mail.com", Password = "Test2026*" };
            var loginRequest = new LoginRequest { Email = "test@mail.com", Password = "Test2026*" };

            var registerResponse = await authenticationService.RegisterUser(registerRequest);

            registerResponse.EnsureSuccessStatusCode();

            var confirmEmailResponse = await authenticationService.ConfirmEmailByFollowingLinkFromTheLastEmail();

            confirmEmailResponse.EnsureSuccessStatusCode();

            var loginResponse = await authenticationService.Login(loginRequest);

            loginResponse.EnsureSuccessStatusCode();

            var loginAuthenticatedUserResponse = await loginResponse.Content.ReadFromJsonAsync<AuthenticatedUserResponse>();

            if (loginAuthenticatedUserResponse?.AccessToken == null)
                throw new Exception();

            var changeEmailRequest = new ChangeEmailRequest() { NewEmail = "" };

            var changeEmailResponse = await authenticationService.ChangeEmail(changeEmailRequest, loginAuthenticatedUserResponse.AccessToken);

            Assert.Equal(HttpStatusCode.BadRequest, changeEmailResponse.StatusCode);

            ProblemDetails? problemDetails = await changeEmailResponse.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problemDetails == null)
                throw new Exception("Should return problem details");

            Assert.NotNull(problemDetails.Extensions["timestamp"]);
            Assert.NotNull(problemDetails.Extensions["errors"]);
            Assert.NotNull(problemDetails.Extensions["traceId"]);

            var messageSummary = await papercutService.GetMessageSummaryAsync();

            if (messageSummary == null)
                throw new Exception();

            Assert.Equal(1, messageSummary.TotalMessageCount);
        }
    }
}
