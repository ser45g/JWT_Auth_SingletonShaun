using System.ComponentModel.DataAnnotations;

namespace MyJwtAuthService.Requests
{
    public class ResetPasswordRequest
    {
        public required string Email { get; set; }

        public required string NewPassword { get; set; }

        public required string ResetCode { get; set; }
    }
}
