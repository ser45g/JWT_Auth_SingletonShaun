using System.ComponentModel.DataAnnotations;

namespace MyJwtAuthService.Options
{
    public class CorsOptions
    {
        [Required]
        public required IEnumerable<string> AllowedOrigins { get; init; } = Enumerable.Empty<string>();
    }
}
