
namespace MyJwtAuthService.Tests.Papercut
{
    public record PapercutMessageDetails(string Id, DateTime CreatedAt, string Subject, List<EmailAddress> From, List<EmailAddress> To, List<EmailAddress> Cc, List<EmailAddress> BCc, string HtmlBody, string TextBody, List<EmailHeader> Headers, List<EmailSection> Sections);
}
