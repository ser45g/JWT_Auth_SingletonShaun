
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MyJwtAuthService.Requests;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.ConfirmEmail
{
    public class ConfirmEmail_IncorrectToken_ShouldReturnUnauthorized_Test : BaseIntegrationTest
    {
        public ConfirmEmail_IncorrectToken_ShouldReturnUnauthorized_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task ConfirmEmail_IncorrectToken_ShouldReturnUnauthorized()
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

            var queryParams = QueryHelpers.ParseQuery(new Uri(link).Query);

            var userId = queryParams["userId"].ToString();
            var code = queryParams["code"].ToString();

            if (string.IsNullOrEmpty(userId))
                throw new Exception(nameof(userId));

            if (string.IsNullOrEmpty(code))
                throw new Exception(nameof(code));

            var confirmationLinkResponse = await authenticationService.ConfirmEmail(userId, "ALSKJFSDJDIUIOUUWEY89797SDSFSFKFJDSDF98DFDFDSJFLJSDF98UF9SDF98DFSJDFSKSDJF9");

            Assert.Equal(HttpStatusCode.Unauthorized, confirmationLinkResponse.StatusCode);

            ProblemDetails? problemDetails = await confirmationLinkResponse.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problemDetails == null)
                throw new Exception("Should return problem details");

            Assert.NotNull(problemDetails.Extensions["timestamp"]);
            Assert.NotNull(problemDetails.Extensions["traceId"]);

        }
    }
}
