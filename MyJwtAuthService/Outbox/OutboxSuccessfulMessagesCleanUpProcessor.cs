using EFCore.BulkExtensions;
using EFCore.PostgresExtensions.Enums;
using EFCore.PostgresExtensions.Extensions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyJwtAuthServer.Contracts.Events.BusinessEvents;
using MyJwtAuthService.Data;
using MyJwtAuthService.Options;
using Polly;
using Polly.Retry;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace MyJwtAuthService.Outbox
{

    public class OutboxSuccessfulMessagesCleanUpProcessor(AppIdentityDbContext dbContext)
    {

        private static readonly AsyncRetryPolicy RetryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(3, t => TimeSpan.FromMilliseconds(t * 150));

        public async Task<int> DeleteSuccessfulMessagesAsync(CancellationToken stoppingToken = default)
        {
            int deletedCount = await RetryPolicy.ExecuteAsync(async ()=>await dbContext.OutboxMessages.Where(m => m.ProcessedOnUtc != null && !m.IsUnprocessable).ExecuteDeleteAsync(cancellationToken: stoppingToken));

            return deletedCount;
        }

    }
}
