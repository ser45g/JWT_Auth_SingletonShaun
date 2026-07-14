using System.ComponentModel.DataAnnotations;

namespace Webhooks.Processing.Options
{
    public class AuthenticationOptions
    {
        public static readonly string ConfigurationSection = nameof(AuthenticationOptions);

        [Required]
        public required string AccessTokenSecret { get; init; }

        [Required]
        public required string Issuer { get; init; }

        [Required]
        public required string Audience { get; init; }

    }
}
