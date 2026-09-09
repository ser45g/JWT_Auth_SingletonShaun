using MyJwtAuthService.Outbox;
using Quartz;

namespace MyJwtAuthService.Jobs
{
    public class OutboxBackgroundJob(OutboxProcessor processor) : IJob
    {
        public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            await processor.ProcessOutboxMessagesAsync(cancellationToken);
        }
    }
}
