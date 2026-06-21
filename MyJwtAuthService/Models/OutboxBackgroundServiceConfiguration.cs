namespace MyJwtAuthService.Models
{
    public class OutboxBackgroundServiceConfiguration
    {
        public int BatchSize { get; set; } = 10;

        public double IntervalSeconds { get; set; } = 1.5; 
    }
}
