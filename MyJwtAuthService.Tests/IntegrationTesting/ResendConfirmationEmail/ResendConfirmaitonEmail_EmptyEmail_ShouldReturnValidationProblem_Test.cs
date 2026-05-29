using Microsoft.AspNetCore.Mvc;
using MyJwtAuthService.Requests;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.ResendConfirmationEmail
{
    public class ResendConfirmaitonEmail_EmptyEmail_ShouldReturnValidationProblem_Test : BaseIntegrationTest
    {
        public ResendConfirmaitonEmail_EmptyEmail_ShouldReturnValidationProblem_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task ResendConfirmaitonEmail_EmptyEmail_ShouldReturnValidationProblem()
        {
            var registerRequest1 = new RegisterRequest { Email = "test@mail.com", Password = "Test2026*" };

            var resendRequest = new ResendRequest { Email = "" };

            var response1 = await authenticationService.RegisterUser(registerRequest1);

            response1.EnsureSuccessStatusCode();

            var resendConfirmationResponse = await authenticationService.ResendConfirmationEmail(resendRequest);

            ProblemDetails? problemDetails = await resendConfirmationResponse.Content.ReadFromJsonAsync<ProblemDetails>();
            if (problemDetails == null)
                throw new Exception(nameof(problemDetails));

            Assert.Equal((int)HttpStatusCode.BadRequest, problemDetails.Status);
            Assert.NotNull(problemDetails.Extensions["errors"]);
            Assert.NotNull(problemDetails.Extensions["traceId"]);
            Assert.NotNull(problemDetails.Extensions["timestamp"]);

            var messageSummary = await papercutService.GetMessageSummaryAsync();

            Assert.NotNull(messageSummary);
            Assert.Equal(1, messageSummary.TotalMessageCount);
        }
    }
}

