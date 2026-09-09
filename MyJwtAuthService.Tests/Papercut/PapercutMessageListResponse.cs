namespace MyJwtAuthService.Tests.Papercut
{
    public record PapercutMessageListResponse(int TotalMessageCount, List<PapercutMessageSummary> Messages);
}
