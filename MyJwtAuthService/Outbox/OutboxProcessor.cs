using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyJwtAuthService.Data;
using MyJwtAuthService.Options;
using System.Reflection;
using System.Text.Json;

namespace MyJwtAuthService.Outbox
{
    public class OutboxProcessor(AppIdentityDbContext dbContext, IPublisher sender, IOptions<OutboxBackgroundServiceOptions> options)
    {
        public async Task<int> ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
        {
            var nonProcessedMessages = await dbContext.OutboxMessages
                .Where(m => m.ProcessedOnUtc == null).OrderBy(m=>m.OccuredOnUtc).Take(options.Value.BatchSize).ToListAsync(stoppingToken);

            if (nonProcessedMessages.Count == 0) {
                return 0;
            }

            var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

            var assembly = Assembly.GetExecutingAssembly();
            foreach (var message in nonProcessedMessages) {
                try
                {
                    Type? msgType = assembly.GetType(message.Type);

                    var deserializedMessage = JsonSerializer.Deserialize(message.Content, msgType);

                    if(deserializedMessage is null)
                        throw new Exception();

                    await sender.Publish(deserializedMessage, cancellationToken: stoppingToken);

                    await dbContext.OutboxMessages.Where(m => m.Id == message.Id).ExecuteUpdateAsync(m => m.SetProperty(m => m.ProcessedOnUtc, DateTime.UtcNow), stoppingToken);


                } catch (Exception ex) {
                    await dbContext.OutboxMessages.Where(m => m.Id == message.Id).ExecuteUpdateAsync(m => m.SetProperty(m => m.Error, ex.Message), stoppingToken);
                }
            }
            await transaction.CommitAsync(stoppingToken);
            return nonProcessedMessages.Count;
        }
    }
}
