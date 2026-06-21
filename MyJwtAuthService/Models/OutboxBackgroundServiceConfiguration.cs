namespace MyJwtAuthService.Models
{
    public class OutboxBackgroundServiceConfiguration
    {
        public int BatchSize { get; set; } = 10;

        public int IntervalMiliseconds { get; set; } = 1000; 
    }
}
