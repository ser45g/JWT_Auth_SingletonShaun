using System.ComponentModel.DataAnnotations;

namespace MyJwtAuthService.Models
{
    public class CorsConfiguration
    {
        [Required]
        public required IEnumerable<string> AllowedOrigins { get; set; }
    }
}
