using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyJwtAuthServer.Contracts.Events;
using MyJwtAuthServer.Contracts.Events.Webhooks;
using Webhooks.Processing.Data;

namespace Webhooks.Processing.Consumers
{
    public class WebhookDispatchedConsumer(WebhooksDbContext dbContext, IPublishEndpoint publishEndpoint) : IConsumer<WebhookDispatchedEvent>
    {
        public async Task Consume(ConsumeContext<WebhookDispatchedEvent> context)
        {
            var @event = context.Message;

            var subscriptions = await dbContext.WebhookSubscriptions.AsNoTracking().Where(s => s.EventType == @event.EventType).ToListAsync(context.CancellationToken);

            if (subscriptions.Count == 0)
            {
                return;
            }

            var publishingEvents = subscriptions.Select(subscription => new WebhookTriggeredEvent(subscription.Id, @event.EventType, subscription.WebhookUrl, @event.Payload));

            await publishEndpoint.PublishBatch(publishingEvents, context.CancellationToken);
        }
    }
}
