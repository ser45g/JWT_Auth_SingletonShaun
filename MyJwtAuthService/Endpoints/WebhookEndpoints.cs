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

            webhooksGroup.MapPost("subscriptions", async Task<Created> (CreateWebhookRequest req, WebhookDispatcher dispatcher, CancellationToken cancellationToken) =>
            {
                await dispatcher.AddSubscription(req.EventType, req.WebhookUrl, cancellationToken);

                return TypedResults.Created();
            });

            webhooksGroup.MapPost("test", async Task<Ok>(WebhookDispatcher dispatcher, CancellationToken cancellationToken) =>
            {
                await dispatcher.DispatchAsync("test.event", new { Message = "This is a test webhook event." }, cancellationToken);
                return TypedResults.Ok();
            });

            return webhooksGroup;
        }
    }
}

     
