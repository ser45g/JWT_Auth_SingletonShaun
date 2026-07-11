using MassTransit;
using Webhooks.Processing.Data;
using Webhooks.Processing.Models;

namespace Webhooks.Processing.Consumers
{
    public class WebhookTriggeredConsumer(IHttpClientFactory httpClientFactory, WebhooksDbContext dbContext) : IConsumer<WebhookTriggered>
    {
        public async Task Consume(ConsumeContext<WebhookTriggered> context)
        {
            var @event = context.Message;

            using var httpClient = httpClientFactory.CreateClient();

            WebhookDeliveryAttempt? deliveryAttempt = null;
            try
            {
                var response = await httpClient.PostAsJsonAsync(@event.WebhookUrl, @event.Data, context.CancellationToken);
                deliveryAttempt = new WebhookDeliveryAttempt(Guid.NewGuid(), @event.SubscriptionId, (int)response.StatusCode, response.IsSuccessStatusCode, DateTime.UtcNow);

            }
            catch (Exception ex)
            {
                deliveryAttempt = new WebhookDeliveryAttempt(Guid.NewGuid(), @event.SubscriptionId, null, false, DateTime.UtcNow);
            }
            finally
            {
                dbContext.WebhookDeliveryAttempts.Add(deliveryAttempt);
                await dbContext.SaveChangesAsync(context.CancellationToken);
            }
        }
    }
}
