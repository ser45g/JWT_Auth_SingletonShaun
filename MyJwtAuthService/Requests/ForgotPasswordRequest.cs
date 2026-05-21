using System.ComponentModel.DataAnnotations;

namespace MyJwtAuthService.Requests
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}
