using MyJwtAuthService.Requests;

namespace MyJwtAuthService.Tests.IntegrationTesting.ForgotPassword
{
    public class ForgotPassword_ShouldMailResetCode_Test : BaseIntegrationTest
    {
        public ForgotPassword_ShouldMailResetCode_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task ForgotPassword_ShouldMailResetCode()
        {
            var registerRequest = new RegisterRequest { Email = "test@mail.com", Password = "Test2026*" };
            var forgotPasswordRequest = new ForgotPasswordRequest() { Email = registerRequest.Email };

            var registerResponse = await authenticationService.RegisterUser(registerRequest);

            registerResponse.EnsureSuccessStatusCode();

            var confirmEmailLink = await papercutService.GetConfirmationLinkFromLastEmailAsync(isDelayed: true);
            WasNullException.ThrowIfNull(confirmEmailLink, nameof(confirmEmailLink));

            var confirmEmailResponse = await authenticationService.ConfirmEmail(confirmEmailLink);
            confirmEmailResponse.EnsureSuccessStatusCode();

            var forgotPasswordResponse = await authenticationService.ForgotPassword(forgotPasswordRequest);

            forgotPasswordResponse.EnsureSuccessStatusCode();

            var code = await papercutService.GetResetPasswordTokenFromLastEmail(isDelayed:true);

            Assert.NotNull(code);
        }
    }
}
