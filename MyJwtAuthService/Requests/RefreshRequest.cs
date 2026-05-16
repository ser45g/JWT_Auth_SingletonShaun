using System.ComponentModel.DataAnnotations;

namespace MyJwtAuthService.Requests
{
    public class RefreshRequest
    {
        [Required]
        public required string RefreshToken { get; set; }
    }
}
