namespace MyJwtAuthService.Options
{
    public class OutboxBackgroundServiceOptions
    {
        public int BatchSize { get; init; } = 10;

        public int IntervalSeconds { get; init; } = 3;

        public int MaxDegreeOfParallelism { get; init; } = 5;
    }
}
