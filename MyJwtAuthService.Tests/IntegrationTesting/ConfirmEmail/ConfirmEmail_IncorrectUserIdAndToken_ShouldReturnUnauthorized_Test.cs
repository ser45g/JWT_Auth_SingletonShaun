using Microsoft.AspNetCore.Mvc;
using MyJwtAuthService.Requests;
using MyJwtAuthService.Responses;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.ConfirmEmail
{
    public class ConfirmEmail_IncorrectUserIdAndToken_ShouldReturnUnauthorized_Test : BaseIntegrationTest
    {
        public ConfirmEmail_IncorrectUserIdAndToken_ShouldReturnUnauthorized_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task ConfirmEmail_IncorrectUserIdAndToken_ShouldReturnUnauthorized()
        {
            var registerRequest = new RegisterRequest { Email = "test@mail.com", Password = "Test2026*" };
            var loginRequest = new LoginRequest { Email = "test@mail.com", Password = "Test2026*" };

            var registerResponse = await authenticationService.RegisterUser(registerRequest);

            registerResponse.EnsureSuccessStatusCode();

            var unsuccessfulLoginResponse = await authenticationService.Login(loginRequest);

            Assert.False(unsuccessfulLoginResponse.IsSuccessStatusCode);

            var link = await papercutService.GetConfirmationLinkFromLastEmailAsync();

            if (link == null)
                throw new Exception();

            var confirmationLinkResponse = await authenticationService.ConfirmEmail(Guid.NewGuid().ToString(), "AFLSLDFKSDJDFLSDFJSDLFJLSDFJJFOIOIUFL");

            Assert.Equal(HttpStatusCode.Unauthorized, confirmationLinkResponse.StatusCode);

            ProblemDetails? problemDetails = await confirmationLinkResponse.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problemDetails == null)
                throw new Exception("Should return problem details");

            Assert.NotNull(problemDetails.Extensions["timestamp"]);
            Assert.NotNull(problemDetails.Extensions["traceId"]);

        }
    }
}
