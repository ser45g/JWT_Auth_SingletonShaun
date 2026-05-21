using System.ComponentModel.DataAnnotations;

namespace MyJwtAuthService.Requests
{
    public class ResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string NewPassword { get; set; }

        [Required]
        public required string ResetCode { get; set; }
    }
}
