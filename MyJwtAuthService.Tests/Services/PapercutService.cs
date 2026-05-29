using MyJwtAuthService.Tests.Papercut;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace MyJwtAuthService.Tests.Services
{
    public class PapercutService
    {
        private readonly IntegrationTestWebAppFactory factory;

        public PapercutService(IntegrationTestWebAppFactory factory)
        {
            this.factory = factory;
        }

        public async Task<string?> GetConfirmationLinkAsync(string messageIdEscaped)
        {
            ArgumentNullException.ThrowIfNull(messageIdEscaped, nameof(messageIdEscaped));

            using var _httpClient = new HttpClient() { BaseAddress = new Uri(factory.MailServerConnectionString) };

            var response = await _httpClient.GetAsync($"/api/messages/{messageIdEscaped}");
            response.EnsureSuccessStatusCode();

            var message = await response.Content.ReadFromJsonAsync<PapercutMessageDetails>();

            // 2. Parse the token from the email body
            if (message?.HtmlBody != null)
            {
                var pattern = @"https?://[^\s]+";
                var match = Regex.Match(message.HtmlBody, pattern);
                var link = match.Success ? match.Value : null;
                return link?.Replace("&amp;", "&");
            }

            return null;
        }

        public async Task<string?> GetConfirmationLinkFromLastEmailAsync()
        {
            var messageSummary = await GetMessageSummaryAsync();

            var message = messageSummary?.Messages.FirstOrDefault();

            if (message?.Id == null)
                throw new Exception(nameof(message));

            return await GetConfirmationLinkAsync(Uri.EscapeDataString(message.Id));

        }

        public async Task<PapercutMessageListResponse?> GetMessageSummaryAsync()
        {
            using var _httpClient = new HttpClient() { BaseAddress = new Uri(factory.MailServerConnectionString) };

            var messagesSummaryResponse = await _httpClient.GetAsync("/api/messages");
            if (messagesSummaryResponse.IsSuccessStatusCode)
            {
                return await messagesSummaryResponse.Content.ReadFromJsonAsync<PapercutMessageListResponse>();
            }
            return null;
        }

        public async Task<string?> GetResetPasswordToken(string messageIdEscaped)
        {
            using var _httpClient = new HttpClient() { BaseAddress = new Uri(factory.MailServerConnectionString) };

            var response = await _httpClient.GetAsync($"/api/messages/{messageIdEscaped}");
            response.EnsureSuccessStatusCode();

            var message = await response.Content.ReadFromJsonAsync<PapercutMessageDetails>();

            // 2. Parse the token from the email body
            if (message?.HtmlBody != null)
            {
                var pattern = @"Here is your reset code: (\w+)";
                var match = Regex.Match(message.HtmlBody, pattern);
                var code = match.Success ? match.Groups[1].Value : null;
                return code;
            }

            return null;
        }

        public async Task<string?> GetResetPasswordTokenFromLastEmail()
        {
            var messageSummary = await GetMessageSummaryAsync();

            var message = messageSummary?.Messages.FirstOrDefault();

            if (message?.Id == null)
                throw new Exception(nameof(message));

            return await GetResetPasswordToken(Uri.EscapeDataString(message.Id));
        }
    }
}
