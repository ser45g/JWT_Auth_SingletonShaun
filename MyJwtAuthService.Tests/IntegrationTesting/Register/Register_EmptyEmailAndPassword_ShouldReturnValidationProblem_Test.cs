using Microsoft.AspNetCore.Mvc;
using MyJwtAuthService.Requests;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.Register
{
    public class Register_EmptyEmailAndPassword_ShouldReturnValidationProblem_Test : BaseIntegrationTest
    {
        public Register_EmptyEmailAndPassword_ShouldReturnValidationProblem_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task Register_EmptyEmailAndPassword_ShouldReturnValidationProblem()
        {
            var registerRequest1 = new RegisterRequest { Email = "", Password = "" };

            var response1 = await authenticationService.RegisterUser(registerRequest1);

            ProblemDetails? problemDetails = await response1.Content.ReadFromJsonAsync<ProblemDetails>();
            if (problemDetails == null)
                throw new Exception(nameof(problemDetails));

            Assert.Equal((int)HttpStatusCode.BadRequest, problemDetails.Status);
            Assert.NotNull(problemDetails.Extensions["errors"]);
            Assert.NotNull(problemDetails.Extensions["traceId"]);
            Assert.NotNull(problemDetails.Extensions["timestamp"]);
        }
    }
}
