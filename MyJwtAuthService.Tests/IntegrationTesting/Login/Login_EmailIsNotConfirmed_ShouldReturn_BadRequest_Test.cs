using Microsoft.AspNetCore.Mvc;
using MyJwtAuthService.Requests;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.Login
{
    public class Login_EmailIsNotConfirmed_ShouldReturn_BadRequest_Test : BaseIntegrationTest
    {

        public Login_EmailIsNotConfirmed_ShouldReturn_BadRequest_Test(IntegrationTestWebAppFactory factory) : base(factory){}

        [Fact]
        public async Task Login_EmailIsNotConfirmed_ShouldReturn_BadRequest()
        {
            var registerRequest = new RegisterRequest { Email = "test@mail.com", Password = "Test2026*" };
            var loginRequest = new LoginRequest { Email = "test@mail.com", Password = "Test2026*" };

            var registerResponse = await authenticationService.RegisterUser(registerRequest);
            registerResponse.EnsureSuccessStatusCode();

            var loginResponse = await authenticationService.Login(loginRequest);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, loginResponse.StatusCode);

            ProblemDetails? problemDetails = await loginResponse.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problemDetails == null)
                throw new Exception("Login should return problem details");

            Assert.NotNull(problemDetails.Extensions["timestamp"]);
            Assert.NotNull(problemDetails.Extensions["traceId"]);
        }
    }
}
