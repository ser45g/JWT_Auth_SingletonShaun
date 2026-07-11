using System.ComponentModel.DataAnnotations;

namespace Webhooks.Processing.Options
{
    public class RabbitMqOptions
    {
        public static readonly string ConfigurationSection = nameof(RabbitMqOptions);

        [Required]
        public required string Url { get; init; }

        [Required]
        public required string Username { get; init; }

        [Required]
        public required string Password { get; init; }

        public bool IsSslUsed { get; init; } = true;
    }
}
