using System.ComponentModel.DataAnnotations;

namespace MyJwtAuthService.Options
{
    public class AuthenticationOptions
    {
        [Required]
        public required string AccessTokenSecret { get; init; }

        [Required]
        public required double AccessTokenExpirationMinutes { get; init; }

        [Required]
        public required string Issuer { get; init; }

        [Required]
        public required string Audience { get; init; }

        [Required]
        public required string RefreshTokenSecret { get; init; }

        [Required]
        public required double RefreshTokenExpirationMinutes { get; init; }
    }
}
