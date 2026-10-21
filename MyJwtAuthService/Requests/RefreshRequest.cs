using System.ComponentModel.DataAnnotations;

namespace MyJwtAuthService.Requests
{
    public class RefreshRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
