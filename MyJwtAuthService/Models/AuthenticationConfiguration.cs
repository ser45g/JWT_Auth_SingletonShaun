namespace MyJwtAuthService.Models
{
    public class AuthenticationConfiguration
    {
        public required string AccessTokenSecret { get; set; }
        public required double AccessTokenExpirationMinutes { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public required string RefreshTokenSecret { get; set; }
        public required double RefreshTokenExpirationMinutes { get; set; }
    }
}
