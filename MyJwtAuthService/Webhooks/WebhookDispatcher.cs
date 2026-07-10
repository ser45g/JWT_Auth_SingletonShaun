using Azure;
using Microsoft.EntityFrameworkCore;
using MyJwtAuthService.Data;
using System.Data.Common;
using System.Diagnostics;

namespace MyJwtAuthService.Webhooks
{
    public class WebhookDispatcher(IHttpClientFactory httpClientFactory, AppIdentityDbContext dbContext)
    {
        public async Task DispatchAsync<T>(string eventType, T payload, CancellationToken cancellationToken=default)
        {
            var subscriptions = await dbContext.WebhookSubscriptions.AsNoTracking().Where(s => s.EventType == eventType).ToListAsync(cancellationToken);
            
            foreach (var subscription in subscriptions) {
                using var httpClient = httpClientFactory.CreateClient();

                var request = new WebhookPayload<T>(Guid.NewGuid(), eventType, subscription.Id, DateTime.UtcNow, payload);

                WebhookDeliveryAttempt? deliveryAttempt = null;
                try
                {
                    var response = await httpClient.PostAsJsonAsync<T>(subscription.WebhookUrl, payload, cancellationToken);
                    deliveryAttempt = new WebhookDeliveryAttempt(Guid.NewGuid(), subscription.Id, (int)response.StatusCode, response.IsSuccessStatusCode, DateTime.UtcNow);

                }
                catch (Exception ex) {
                    deliveryAttempt = new WebhookDeliveryAttempt(Guid.NewGuid(), subscription.Id, null, false, DateTime.UtcNow);
                }
                finally
                {
                    dbContext.WebhookDeliveryAttempts.Add(deliveryAttempt);
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
            }

        }
    }
}
