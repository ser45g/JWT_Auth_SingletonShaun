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
    public class OutboxProcessor(AppIdentityDbContext dbContext, IPublishEndpoint sender, IOptions<OutboxBackgroundServiceOptions> options)
    {
        private static readonly ConcurrentDictionary<string, Type> TypeCache = new();

        private static readonly AsyncRetryPolicy RetryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(3, t => TimeSpan.FromMilliseconds(t * 150));

        private static Type? GetOrAddMessageType(string typeName, Assembly assembly)
        {
            var type = assembly.GetType(typeName);

            return type != null ? TypeCache.GetOrAdd(typeName, type) : null;
        }

        public async Task<int> ProcessOutboxMessagesAsync(CancellationToken stoppingToken = default)
        {
            List<OutboxMessage> nonProcessedMessages = await dbContext.OutboxMessages.AsNoTracking().Where(m => m.ProcessedOnUtc == null).OrderBy(m => m.OccuredOnUtc).Take(options.Value.BatchSize).ForUpdate<OutboxMessage>(LockBehavior.SkipLocked).ToListAsync(stoppingToken);

            if (nonProcessedMessages.Count == 0)
            {
                return 0;
            }
            var updateQueue = new Queue<(OutboxMessage, object?)>();

            var assembly = Assembly.GetAssembly(typeof(RegistrationEmailConfirmationSentEvent)) ?? throw new Exception("assembly was null");

            nonProcessedMessages.ForEach(x => EnqueueMessage(x, updateQueue, assembly));

            var entities = updateQueue.Select(x => x.Item1);

            IEnumerable<object> events = updateQueue.Select(x => x.Item2).Where(x=>x!=null)!;

            await RetryPolicy.ExecuteAsync(async () =>
                await sender.PublishBatch(events, cancellationToken: stoppingToken));

            await using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

            await RetryPolicy.ExecuteAsync(async () =>
                await dbContext.BulkUpdateAsync(entities, new BulkConfig() { 
                    PropertiesToInclude = new List<string> {
                        nameof(OutboxMessage.ProcessedOnUtc),
                        nameof(OutboxMessage.Error) 
                    } 
                }, cancellationToken: stoppingToken));

            await transaction.CommitAsync(stoppingToken);

            return nonProcessedMessages.Count;
        }

        public static void EnqueueMessage(OutboxMessage message, Queue<(OutboxMessage, object?)> updateQueue, Assembly assembly)
        {
            try
            {
                Type? msgType = GetOrAddMessageType(message.Type, assembly);

                var deserializedMessage = JsonSerializer.Deserialize(message.Content, msgType) ?? throw new Exception("Could not deserialize the message");

                updateQueue.Enqueue((message with { ProcessedOnUtc = DateTime.UtcNow }, deserializedMessage));
            }
            catch (Exception ex)
            {
                updateQueue.Enqueue((message with { ProcessedOnUtc = null, Error = ex.ToString() }, null));
            }
        }
    }
}
