using MyJwtAuthService.Requests;
using MyJwtAuthService.Responses;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.ResetPassword
{
    public class ResetPassword_ShouldResetPassword_Test : BaseIntegrationTest
    {
        public ResetPassword_ShouldResetPassword_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task ResetPassword_ShouldResetPassword()
        {
            var registerRequest = new RegisterRequest { Email = "test@mail.com", Password = "Test2026*" };
            var loginRequest = new LoginRequest { Email = "test@mail.com", Password = "Test2026*" };

            var registerResponse = await authenticationService.RegisterUser(registerRequest);

            registerResponse.EnsureSuccessStatusCode();

            var confirmEmailResponse = await authenticationService.ConfirmEmailByFollowingLinkFromTheLastEmail();

            confirmEmailResponse.EnsureSuccessStatusCode();

            var loginResponse = await authenticationService.Login(loginRequest);

            loginResponse.EnsureSuccessStatusCode();

            var forgotPasswordRequest = new ForgotPasswordRequest() { Email = registerRequest.Email };

            var forgotPasswordResponse = await authenticationService.ForgotPassword(forgotPasswordRequest);

            forgotPasswordResponse.EnsureSuccessStatusCode();

            var code = await papercutService.GetResetPasswordTokenFromLastEmail();
            if (code == null)
                throw new Exception(nameof(code));

            var resetPasswordRequest = new ResetPasswordRequest() { Email= registerRequest.Email, ResetCode=code, NewPassword="Test882026***" };

            var resetPasswordResponse = await authenticationService.ResetPassword(resetPasswordRequest);

            Assert.Equal(HttpStatusCode.OK, resetPasswordResponse.StatusCode);

            var loginWithNewPasswordRequest = new LoginRequest() { Email = registerRequest.Email, Password = resetPasswordRequest.NewPassword };

            var loginWithNewPasswordResponse = await authenticationService.Login(loginWithNewPasswordRequest);

            Assert.Equal(HttpStatusCode.OK, loginWithNewPasswordResponse.StatusCode);

            var authenticatedUserResponse = await loginWithNewPasswordResponse.Content.ReadFromJsonAsync<AuthenticatedUserResponse>();

            Assert.NotNull(authenticatedUserResponse);
            Assert.NotEmpty(authenticatedUserResponse.AccessToken);
            Assert.NotEqual(DateTime.MinValue, authenticatedUserResponse.AccessTokenExpirationTime);
            Assert.NotEmpty(authenticatedUserResponse.RefreshToken);
        }
    }
}
