using System.Diagnostics;

namespace MyJwtAuthService.Helpers
{
    public static class DiagnosticConfig
    {
        public static readonly ActivitySource ActivitySource = new("webhook-api");
    }
}
