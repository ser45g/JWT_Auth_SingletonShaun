using MassTransit;
using Webhooks.Processing.Data;
using Webhooks.Processing.Models;

namespace Webhooks.Processing.Consumers
{
    public class WebhookSubscriptionAddedConsumer(WebhooksDbContext dbContext) : IConsumer<WebhookSubscriptionAdded>
    {
        public async Task Consume(ConsumeContext<WebhookSubscriptionAdded> context)
        {
            var @event = context.Message;

            var subscription = new WebhookSubscription(Guid.NewGuid(), @event.EventType, @event.WebhookUrl, DateTime.UtcNow);

            dbContext.WebhookSubscriptions.Add(subscription);
            
            await dbContext.SaveChangesAsync();
        }
    }
}
