using MyJwtAuthService.Requests;
using System.Net;

namespace MyJwtAuthService.Tests.IntegrationTesting.ForgotPassword
{
    public class ForgotPassword_IncorrectEmail_ShouldReturnOk_Test : BaseIntegrationTest
    {
        public ForgotPassword_IncorrectEmail_ShouldReturnOk_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task ForgotPassword_IncorrectEmail_ShouldReturnOk()
        {
            var registerRequest = new RegisterRequest { Email = "test@mail.com", Password = "Test2026*" };
            var forgotPasswordRequest = new ForgotPasswordRequest() { Email = "test77@mail.com" };

            var registerResponse = await authenticationService.RegisterUser(registerRequest);

            registerResponse.EnsureSuccessStatusCode();

            var emailConfirmationResponse = await authenticationService.ConfirmEmailByFollowingLinkFromTheLastEmail();

            emailConfirmationResponse.EnsureSuccessStatusCode();

            var forgotPasswordResponse = await authenticationService.ForgotPassword(forgotPasswordRequest);

            Assert.Equal(HttpStatusCode.OK, forgotPasswordResponse.StatusCode);

            var messagesSummary = await papercutService.GetMessageSummaryAsync();

            if (messagesSummary == null)
                throw new Exception(nameof(messagesSummary));

            Assert.Equal(1, messagesSummary.TotalMessageCount);
        }
    }
}
