
using MyJwtAuthService.Tests.Papercut;

namespace MyJwtAuthService.Tests.Services
{
    public class DelayedForOutboxPapercutService(IPapercutService papercutService)
    {
        public async Task<string?> GetConfirmationLinkAsync(string messageIdEscaped, bool isDelayed=false)
        {
            if(isDelayed)
                await Task.Delay(IntegrationTestWebAppFactory.OutboxDelayMiliseconds);

            return await papercutService.GetConfirmationLinkAsync(messageIdEscaped);
        }

        public async Task<string?> GetConfirmationLinkFromLastEmailAsync(bool isDelayed = false)
        {
            if (isDelayed)
                await Task.Delay(IntegrationTestWebAppFactory.OutboxDelayMiliseconds);

            return await  papercutService.GetConfirmationLinkFromLastEmailAsync();
        }

        public async Task<PapercutMessageListResponse?> GetMessageSummaryAsync(bool isDelayed = false)
        {
            if (isDelayed)
                await Task.Delay(IntegrationTestWebAppFactory.OutboxDelayMiliseconds);

            return await papercutService.GetMessageSummaryAsync();
        }

        public async Task<string?> GetResetPasswordToken(string messageIdEscaped, bool isDelayed = false)
        {
            if (isDelayed)
                await Task.Delay(IntegrationTestWebAppFactory.OutboxDelayMiliseconds);

            return await papercutService.GetResetPasswordToken(messageIdEscaped);
        }

        public async Task<string?> GetResetPasswordTokenFromLastEmail(bool isDelayed = false)
        {
            if (isDelayed)
                await Task.Delay(IntegrationTestWebAppFactory.OutboxDelayMiliseconds);

            return await papercutService.GetResetPasswordTokenFromLastEmail();
        }
    }
}
