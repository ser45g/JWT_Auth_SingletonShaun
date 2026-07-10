using Microsoft.AspNetCore.Http.HttpResults;
using MyJwtAuthService.Data;
using MyJwtAuthService.Requests;
using MyJwtAuthService.Webhooks;

namespace MyJwtAuthService.Endpoints
{
    public static class WebhookEndpoints
    {
        public static IEndpointRouteBuilder AddWebhookEndpoints(this IEndpointRouteBuilder app)
        {
            var webhooksGroup = app.MapGroup("webhooks");

            webhooksGroup.MapPost("subscriptions", async Task<Created> (CreateWebhookRequest req, AppIdentityDbContext dbContext) =>
            {
                var webhookSubscription = new WebhookSubscription(Guid.NewGuid(), req.EventType, req.WebhookUrl, DateTime.UtcNow);

                dbContext.WebhookSubscriptions.Add(webhookSubscription);

                await dbContext.SaveChangesAsync();

                return TypedResults.Created();
            });

            webhooksGroup.MapPost("test", async Task<Ok>(WebhookDispatcher dispatcher) =>
            {
                await dispatcher.DispatchAsync("test.event", new { Message = "This is a test webhook event." });
                return TypedResults.Ok();
            });

            return webhooksGroup;
        }
    }
}

     
