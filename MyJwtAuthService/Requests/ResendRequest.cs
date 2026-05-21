using System.ComponentModel.DataAnnotations;

namespace MyJwtAuthService.Requests
{
    public class ResendRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}
