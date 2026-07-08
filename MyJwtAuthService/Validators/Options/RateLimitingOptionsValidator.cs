using FluentValidation;
using MyJwtAuthService.Options;

namespace MyJwtAuthService.Validators.Options
{
    public class RateLimitingOptionsValidator:AbstractValidator<RateLimitingOptions>
    {
        public RateLimitingOptionsValidator() {
            RuleFor(x => x.GlobalRateLimiter).NotNull().ChildRules(global =>
            {
                global.RuleFor(x => x.PermitLimit).GreaterThan(0);
                global.RuleFor(x => x.Window).GreaterThan(TimeSpan.Zero);
                global.RuleFor(x => x.QueueLimit).GreaterThanOrEqualTo(0);
            });

            RuleFor(x => x.GlobalRateLimiter).NotNull().ChildRules(ip =>
            {
                ip.RuleFor(x => x.PermitLimit).GreaterThan(0);
                ip.RuleFor(x => x.Window).GreaterThan(TimeSpan.Zero);
                ip.RuleFor(x => x.QueueLimit).GreaterThanOrEqualTo(0);
            });
        }
    }
}
