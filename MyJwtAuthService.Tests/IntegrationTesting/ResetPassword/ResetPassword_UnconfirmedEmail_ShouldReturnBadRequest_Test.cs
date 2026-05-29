using Microsoft.AspNetCore.Mvc;
using MyJwtAuthService.Requests;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.ResetPassword
{
    public class ResetPassword_UnconfirmedEmail_ShouldReturnBadRequest_Test : BaseIntegrationTest
    {
        public ResetPassword_UnconfirmedEmail_ShouldReturnBadRequest_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task ResetPassword_IncorrectResetToken_ShouldReturnBadRequest()
        {
            var registerRequest = new RegisterRequest { Email = "test@mail.com", Password = "Test2026*" };
            var loginRequest = new LoginRequest { Email = "test@mail.com", Password = "Test2026*" };

            var registerResponse = await authenticationService.RegisterUser(registerRequest);

            registerResponse.EnsureSuccessStatusCode();

            var loginResponse = await authenticationService.Login(loginRequest);

            Assert.False(loginResponse.IsSuccessStatusCode);

            var forgotPasswordRequest = new ForgotPasswordRequest() { Email = registerRequest.Email };

            var forgotPasswordResponse = await authenticationService.ForgotPassword(forgotPasswordRequest);

            Assert.False(loginResponse.IsSuccessStatusCode);

            var resetPasswordRequest = new ResetPasswordRequest() { Email = registerRequest.Email, ResetCode = "myVeryGoodRESETTOKENFORAUTHENTICATION", NewPassword = "Test882026***" };

            var resetPasswordResponse = await authenticationService.ResetPassword(resetPasswordRequest);

            Assert.Equal(HttpStatusCode.BadRequest, resetPasswordResponse.StatusCode);

            ProblemDetails? problemDetails = await resetPasswordResponse.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problemDetails == null)
                throw new Exception("Should return problem details");

            Assert.NotNull(problemDetails.Extensions["timestamp"]);
            Assert.NotNull(problemDetails.Extensions["traceId"]);
        }
    }
}
