using Microsoft.EntityFrameworkCore;
using MyJwtAuthService.Requests;

namespace MyJwtAuthService.Tests.IntegrationTesting.Register
{
    public class Register_ShouldAddNewUserAndSendConfirmationEmail_Test : BaseIntegrationTest
    {

        public Register_ShouldAddNewUserAndSendConfirmationEmail_Test(IntegrationTestWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task Register_ShouldAddNewUserAndSendConfirmationEmail()
        {
            var registerRequest = new RegisterRequest { Email = "test@mail.com", Password = "Test2026*" };

            var response = await authenticationService.RegisterUser(registerRequest);

            response.EnsureSuccessStatusCode();

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == registerRequest.Email);

            Assert.NotNull(user);

            var messagesSummary = await papercutService.GetMessageSummaryAsync();

            Assert.NotNull(messagesSummary);
            Assert.Equal(1, messagesSummary.TotalMessageCount);
        }
    }
}
