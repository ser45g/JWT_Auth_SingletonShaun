using FluentValidation.Results;
using MyJwtAuthService.Options;
using MyJwtAuthService.Services.EmailSenders;

namespace MyJwtAuthService.Extensions
{
    public static class ValidationExtensions
    {
        public static IServiceCollection AddValidationOptions(this IServiceCollection services)
        {
            services.AddOptions<CorsOptions>().BindConfiguration("Cors").ValidateDataAnnotations().ValidateOnStart();

            services.AddOptions<AuthenticationOptions>().BindConfiguration("Authentication").ValidateDataAnnotations().ValidateOnStart();

            services.AddOptions<MailSettings>().BindConfiguration("MailSettings").ValidateDataAnnotations().ValidateOnStart();

            services.AddOptions<OutboxBackgroundServiceOptions>().BindConfiguration("OutboxBackgroundService").ValidateDataAnnotations().ValidateOnStart();

            return services;
        }

        public static IDictionary<string, string[]> GetValidationErrors(this ValidationResult validationResult)
        {
            return validationResult.Errors.GroupBy(x => x.PropertyName).ToDictionary(x => x.Key, x => x.Select(x => x.ErrorMessage).ToArray());
        }
    }
}
