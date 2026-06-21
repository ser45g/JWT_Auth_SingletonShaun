using System.ComponentModel.DataAnnotations;

namespace MyJwtAuthService.Options
{
    public class AuthenticationOptions
    {
        [Required]
        public required string AccessTokenSecret { get; set; }

        [Required]
        public required double AccessTokenExpirationMinutes { get; set; }

        [Required]
        public required string Issuer { get; set; }

        [Required]
        public required string Audience { get; set; }

        [Required]
        public required string RefreshTokenSecret { get; set; }

        [Required]
        public required double RefreshTokenExpirationMinutes { get; set; }
    }
}
