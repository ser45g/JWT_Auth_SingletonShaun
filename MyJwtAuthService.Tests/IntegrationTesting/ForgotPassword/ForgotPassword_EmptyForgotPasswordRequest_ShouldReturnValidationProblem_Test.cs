
using Microsoft.AspNetCore.Mvc;
using MyJwtAuthService.Requests;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.ForgotPassword
{
    public class ForgotPassword_EmptyForgotPasswordRequest_ShouldReturnValidationProblem_Test : BaseIntegrationTest
    {
        public ForgotPassword_EmptyForgotPasswordRequest_ShouldReturnValidationProblem_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task ForgotPassword_EmptyForgotPasswordRequest_ShouldReturnValidationProblem()
        {
            var registerRequest = new RegisterRequest { Email = "test@mail.com", Password = "Test2026*" };
            var forgotPasswordRequest = new ForgotPasswordRequest() { Email = "" };

            var registerResponse = await authenticationService.RegisterUser(registerRequest);

            registerResponse.EnsureSuccessStatusCode();

            var emailConfirmationResponse = await authenticationService.ConfirmEmailByFollowingLinkFromTheLastEmail();

            emailConfirmationResponse.EnsureSuccessStatusCode();

            var forgotPasswordResponse = await authenticationService.ForgotPassword(forgotPasswordRequest);

            Assert.Equal(HttpStatusCode.BadRequest, forgotPasswordResponse.StatusCode);

            ProblemDetails? problemDetails = await forgotPasswordResponse.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problemDetails == null)
                throw new Exception("Should return problem details");

            Assert.NotNull(problemDetails.Extensions["timestamp"]);
            Assert.NotNull(problemDetails.Extensions["traceId"]);
            Assert.NotNull(problemDetails.Extensions["errors"]);

            var code = await papercutService.GetResetPasswordTokenFromLastEmail();

            var messagesSummary = await papercutService.GetMessageSummaryAsync();

            if (messagesSummary == null)
                throw new Exception(nameof(messagesSummary));

            Assert.Equal(1, messagesSummary.TotalMessageCount);
        }
    }
}
