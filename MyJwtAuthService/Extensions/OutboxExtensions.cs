using MyJwtAuthService.Data;
using MyJwtAuthService.Models;
using MyJwtAuthService.Outbox;
using System.Text.Json;

namespace MyJwtAuthService.Extensions
{
    public static class OutboxExtensions
    {
        public static async Task InsertOutboxMessage<T>(this AppIdentityDbContext dbContext, T message, CancellationToken cancellationToken=default) where T: notnull {

            dbContext.InsertOutboxMessageWithoutSaveChanges(message);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public static void InsertOutboxMessageWithoutSaveChanges<T>(this AppIdentityDbContext dbContext, T message) where T : notnull
        {

            ArgumentNullException.ThrowIfNull(message);

            var outboxMessage = new OutboxMessage(Guid.NewGuid(), message.GetType().FullName!, JsonSerializer.Serialize<T>(message), DateTime.UtcNow);

            dbContext.OutboxMessages.Add(outboxMessage);
        }

        public static async Task InsertOutboxMessages(this AppIdentityDbContext dbContext, CancellationToken cancellationToken = default, params object[] messages)
        {
            dbContext.InsertOutboxMessagesWithoutSaveChanges(messages);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public static void InsertOutboxMessagesWithoutSaveChanges(this AppIdentityDbContext dbContext, params object[] messages)
        {
            var containsNullMessages = messages.Any(x => x is null);

            if (containsNullMessages)
            {
                throw new ArgumentNullException(nameof(messages), "Messages cannot contain null values.");
            }

            var outboxMessages = messages.Select(message => new OutboxMessage(Guid.NewGuid(), message.GetType().FullName!, JsonSerializer.Serialize(message), DateTime.UtcNow)).ToList();
            dbContext.OutboxMessages.AddRange(outboxMessages);
        }
    }
}
