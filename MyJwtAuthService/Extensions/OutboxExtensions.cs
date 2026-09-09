using MyJwtAuthService.Data;
using MyJwtAuthService.Models;
using MyJwtAuthService.Outbox;
using System.Text.Json;

namespace MyJwtAuthService.Extensions
{
    public static class OutboxExtensions
    {
        public static async Task InsertOutboxMessage<T>(this AppIdentityDbContext dbContext, T message, CancellationToken cancellationToken=default) where T: notnull {

            ArgumentNullException.ThrowIfNull(message);

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = message.GetType().FullName!,
                Content = JsonSerializer.Serialize<T>(message),
                OccuredOnUtc = DateTime.UtcNow
            };

            dbContext.OutboxMessages.Add(outboxMessage);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
