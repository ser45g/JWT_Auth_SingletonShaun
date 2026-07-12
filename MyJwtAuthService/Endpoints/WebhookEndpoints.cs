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
            }).RequireAuthorization("webhook-subscriber").WithName("add-subscription").WithDescription("Allows users to add a webhook subscription");

            return webhooksGroup;
        }
    }
}

     
