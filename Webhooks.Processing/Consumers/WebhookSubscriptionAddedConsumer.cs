using MassTransit;
using MyJwtAuthServer.Contracts.Events;
using Webhooks.Processing.Data;
using Webhooks.Processing.Models;

namespace Webhooks.Processing.Consumers
{
    public class WebhookSubscriptionAddedConsumer(WebhooksDbContext dbContext) : IConsumer<WebhookSubscriptionAddedEvent>
    {
        public async Task Consume(ConsumeContext<WebhookSubscriptionAddedEvent> context)
        {
            var @event = context.Message;

            var subscription = new WebhookSubscription(Guid.NewGuid(), @event.EventType, @event.WebhookUrl, DateTime.UtcNow);

            dbContext.WebhookSubscriptions.Add(subscription);
            
            await dbContext.SaveChangesAsync();
        }
    }
}
