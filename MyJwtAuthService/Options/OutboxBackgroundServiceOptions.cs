namespace MyJwtAuthService.Options
{
    public class OutboxBackgroundServiceOptions
    {
        public int BatchSize { get; set; } = 10;

        public int IntervalMiliseconds { get; set; } = 1000; 
    }
}
