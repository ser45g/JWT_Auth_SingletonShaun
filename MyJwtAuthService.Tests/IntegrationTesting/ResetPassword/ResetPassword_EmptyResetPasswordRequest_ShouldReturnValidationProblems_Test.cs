using Microsoft.AspNetCore.Mvc;
using MyJwtAuthService.Requests;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.ResetPassword
{
    public class ResetPassword_EmptyResetPasswordRequest_ShouldReturnValidationProblems_Test : BaseIntegrationTest
    {
        public ResetPassword_EmptyResetPasswordRequest_ShouldReturnValidationProblems_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task ResetPassword_EmptyResetPasswordRequest_ShouldReturnValidationProblems()
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

            var resetPasswordRequest = new ResetPasswordRequest() { Email = "", ResetCode = "", NewPassword = "" };

            var resetPasswordResponse = await authenticationService.ResetPassword(resetPasswordRequest);

            ProblemDetails? problemDetails = await resetPasswordResponse.Content.ReadFromJsonAsync<ProblemDetails>();
            if (problemDetails == null)
                throw new Exception(nameof(problemDetails));

            Assert.Equal((int)HttpStatusCode.BadRequest, problemDetails.Status);
            Assert.NotNull(problemDetails.Extensions["errors"]);
            Assert.NotNull(problemDetails.Extensions["traceId"]);
            Assert.NotNull(problemDetails.Extensions["timestamp"]);
        }
    }
}
