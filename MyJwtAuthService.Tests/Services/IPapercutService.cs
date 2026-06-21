using MyJwtAuthService.Tests.Papercut;

namespace MyJwtAuthService.Tests.Services
{
    public interface IPapercutService
    {
        Task<string?> GetConfirmationLinkAsync(string messageIdEscaped);
        Task<string?> GetConfirmationLinkFromLastEmailAsync();
        Task<PapercutMessageListResponse?> GetMessageSummaryAsync();
        Task<string?> GetResetPasswordToken(string messageIdEscaped);
        Task<string?> GetResetPasswordTokenFromLastEmail();
    }
}