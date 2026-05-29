using Microsoft.AspNetCore.Mvc;
using MyJwtAuthService.Requests;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.Login
{
    public class Login_IsLockedOut_ShouldReturn_Unauthorized_Test : BaseIntegrationTest
    {
        public Login_IsLockedOut_ShouldReturn_Unauthorized_Test(IntegrationTestWebAppFactory factory):base(factory){}

        [Fact]
        public async Task Login_IsLockedOut_ShouldReturn_Unauthorized()
        {
            var registerRequest = new RegisterRequest { Email = "test@mail.com", Password = "Test2026*" };
            var correctLoginRequest = new LoginRequest { Email = "test@mail.com", Password = "Test2026*" };
            var incorrectLoginRequest = new LoginRequest { Email = "test@mail.com", Password = "Test2026**dfjl*" };

            var registerResponse = await authenticationService.RegisterUser(registerRequest);
            registerResponse.EnsureSuccessStatusCode();

            var confirmEmailResponse = await authenticationService.ConfirmEmailByFollowingLinkFromTheLastEmail();

            confirmEmailResponse.EnsureSuccessStatusCode();

            HttpResponseMessage correctLoginResponse1 = await authenticationService.Login(correctLoginRequest);
            correctLoginResponse1.EnsureSuccessStatusCode();

            for (int i = 0; i < 5; i++)
            {
                await authenticationService.Login(incorrectLoginRequest);
            }
           
            HttpResponseMessage correctLoginResponse2 = await authenticationService.Login(correctLoginRequest);

            Assert.Equal(HttpStatusCode.Unauthorized, correctLoginResponse2.StatusCode);

            ProblemDetails? problemDetails = await correctLoginResponse2.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problemDetails == null)
                throw new Exception("Login should return problem details");

            Assert.NotNull(problemDetails.Extensions["timestamp"]);
            Assert.NotNull(problemDetails.Extensions["traceId"]);
        }
    }
}
