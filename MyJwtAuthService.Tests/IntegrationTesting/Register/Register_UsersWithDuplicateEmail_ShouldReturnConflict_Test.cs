using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyJwtAuthService.Requests;
using System.Net;
using System.Net.Http.Json;

namespace MyJwtAuthService.Tests.IntegrationTesting.Register
{
    public class Register_UsersWithDuplicateEmail_ShouldReturnConflict_Test : BaseIntegrationTest
    {

        public Register_UsersWithDuplicateEmail_ShouldReturnConflict_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task Register_UsersWithDuplicateEmail_ShouldReturnConflict()
        {
            var registerRequest = new RegisterRequest { Email = "test@mail.com", Password = "Test2026*" };

            var response1 = await authenticationService.RegisterUser(registerRequest);

            response1.EnsureSuccessStatusCode();

            var response2 = await authenticationService.RegisterUser(registerRequest);

            Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);

            var userCount = await _dbContext.Users.CountAsync();

            Assert.Equal(1, userCount);

            ProblemDetails? problemDetails = await response2.Content.ReadFromJsonAsync<ProblemDetails>();
            if (problemDetails == null)
                throw new Exception(nameof(problemDetails));

            Assert.NotNull(problemDetails.Extensions["timestamp"]);
            Assert.NotNull(problemDetails.Extensions["traceId"]);
        }
    }
}
