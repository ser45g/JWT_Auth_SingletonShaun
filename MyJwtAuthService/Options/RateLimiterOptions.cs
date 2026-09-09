using System.Threading.RateLimiting;

namespace MyJwtAuthService.Options
{
    public class RateLimitingOptions
    {
        public required FixedWindowRateLimiterOptions GlobalRateLimiter { get; init; } 
        public required FixedWindowRateLimiterOptions IpRateLimiter { get; init; } 
    }

}
