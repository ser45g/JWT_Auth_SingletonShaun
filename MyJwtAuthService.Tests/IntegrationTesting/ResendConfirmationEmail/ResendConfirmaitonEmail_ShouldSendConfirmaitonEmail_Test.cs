using MyJwtAuthService.Requests;
using System.Net;

namespace MyJwtAuthService.Tests.IntegrationTesting.ResendConfirmationEmail
{
    public class ResendConfirmaitonEmail_ShouldSendConfirmaitonEmail_Test : BaseIntegrationTest
    {
        public ResendConfirmaitonEmail_ShouldSendConfirmaitonEmail_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task ResendConfirmaitonEmail_ShouldSendConfirmaitonEmail()
        {
            var registerRequest1 = new RegisterRequest { Email = "test@mail.com", Password = "Test2026*" };

            var resendRequest = new ResendRequest { Email = "test@mail.com" };

            var response1 = await authenticationService.RegisterUser(registerRequest1);

            response1.EnsureSuccessStatusCode();

            var resendConfirmationResponse = await authenticationService.ResendConfirmationEmail(resendRequest);

            Assert.Equal(HttpStatusCode.OK, resendConfirmationResponse.StatusCode);

            var messageSummary = await papercutService.GetMessageSummaryAsync();

            Assert.NotNull(messageSummary);
            Assert.Equal(2, messageSummary.TotalMessageCount);
        }
    }
}
