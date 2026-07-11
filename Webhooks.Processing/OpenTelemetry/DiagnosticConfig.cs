using System.Diagnostics;

namespace Webhooks.Processing.OpenTelemetry
{
    public static class DiagnosticConfig
    {
        public static readonly ActivitySource ActivitySource = new("webhook-processing");
    }
}
