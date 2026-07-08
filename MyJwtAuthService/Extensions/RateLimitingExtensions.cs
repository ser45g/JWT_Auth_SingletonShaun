using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyJwtAuthService.Helpers;
using MyJwtAuthService.Options;
using System.Threading.RateLimiting;

namespace MyJwtAuthService.Extensions
{
    public static class RateLimitingExtensions
    {
        public static IServiceCollection AddAppRateLimiting(this IServiceCollection services, RateLimitingOptions options)
        {
            services.AddRateLimiter(o =>
            {
                o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    string partitionKey = httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString();

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: partitionKey,
                        factory: (partition) => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = options.GlobalRateLimiter.AutoReplenishment,
                            PermitLimit = options.GlobalRateLimiter.PermitLimit,
                            QueueLimit = options.GlobalRateLimiter.QueueLimit,
                            Window = options.GlobalRateLimiter.Window
                        });
                });

                o.AddPolicy(RateLimitingPolicyNames.IpLimiter, httpContext =>
                {
                    return RateLimitPartition.GetFixedWindowLimiter(partitionKey: httpContext.Connection.RemoteIpAddress, factory: partition => new FixedWindowRateLimiterOptions()
                    {
                        AutoReplenishment = options.IpRateLimiter.AutoReplenishment,
                        PermitLimit = options.IpRateLimiter.PermitLimit,
                        QueueLimit = options.IpRateLimiter.QueueLimit,
                        Window = options.IpRateLimiter.Window
                    });
                });

                o.OnRejected = async (context, ct) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter = $"{retryAfter.TotalSeconds}";
                        ProblemDetailsFactory problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

                        ProblemDetails problemDetails = problemDetailsFactory.CreateProblemDetails(context.HttpContext, StatusCodes.Status429TooManyRequests, "Too many requests", detail: $"Too many requests. Please try again in {retryAfter.TotalSeconds} seconds.");

                        await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, ct);
                    }
                };
            });


            return services;
        }
    }
}
