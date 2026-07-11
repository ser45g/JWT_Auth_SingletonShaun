using MassTransit;
using Microsoft.EntityFrameworkCore;
using Webhooks.Processing.Data;

namespace Webhooks.Processing.Consumers
{
    public class WebhookDispatchedConsumer(WebhooksDbContext dbContext, IPublishEndpoint publishEndpoint) : IConsumer<WebhookDispatched>
    {
        public async Task Consume(ConsumeContext<WebhookDispatched> context)
        {
            var @event = context.Message;

            var subscriptions = await dbContext.WebhookSubscriptions.AsNoTracking().Where(s => s.EventType == @event.EventType).ToListAsync(context.CancellationToken);

            var publishingEvents = subscriptions.Select(subscription => new WebhookTriggered(subscription.Id, @event.EventType, subscription.WebhookUrl, @event.Payload));

            await publishEndpoint.PublishBatch(publishingEvents, context.CancellationToken);
        }
    }
}
