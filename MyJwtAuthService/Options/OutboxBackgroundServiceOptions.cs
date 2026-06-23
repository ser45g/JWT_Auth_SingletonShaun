namespace MyJwtAuthService.Options
{
    public class OutboxBackgroundServiceOptions
    {
        public int BatchSize { get; init; } = 10;

        public int IntervalMiliseconds { get; init; } = 1000;

        public int MaxDegreeOfParallelism { get; init; } = 5;
    }
}
