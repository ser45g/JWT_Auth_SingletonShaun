using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using MyJwtAuthService.Options;
using MyJwtAuthService.Services.EmailSenders;
using MyJwtAuthService.Validators.Options;

namespace MyJwtAuthService.Extensions
{
    public static class ValidationExtensions
    {
        public static IServiceCollection AddValidationOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<CorsOptions>().BindConfiguration("Cors").ValidateDataAnnotations().ValidateOnStart();

            services.AddOptions<AuthenticationOptions>().BindConfiguration("Authentication").ValidateDataAnnotations().ValidateOnStart();

            services.AddOptions<MailSettings>().BindConfiguration("MailSettings").ValidateDataAnnotations().ValidateOnStart();

            services.AddOptions<OutboxBackgroundServiceOptions>().BindConfiguration("OutboxBackgroundService").ValidateDataAnnotations().ValidateOnStart();

            services.Configure<IdentityOptions>(configuration.GetSection(nameof(IdentityOptions)));

            services.AddOptions<RateLimitingOptions>().BindConfiguration("RateLimitingOptions").Validate(x =>
            {
                var validator = new RateLimitingOptionsValidator();
                var result = validator.Validate(x);
                return result.IsValid;

            }).ValidateOnStart();

            return services;
        }

        public static IDictionary<string, string[]> GetValidationErrors(this ValidationResult validationResult)
        {
            return validationResult.Errors.GroupBy(x => x.PropertyName).ToDictionary(x => x.Key, x => x.Select(x => x.ErrorMessage).ToArray());
        }
        public static IDictionary<string, string[]> GetValidationErrors(this IdentityResult identityResult)
        {
            return identityResult.Errors.GroupBy(x=>x.Code).ToDictionary(x => x.Key, x => x.Select(x=>x.Description).ToArray());
        }
    }
}
