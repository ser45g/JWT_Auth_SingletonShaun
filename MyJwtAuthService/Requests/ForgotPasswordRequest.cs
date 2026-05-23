using System.ComponentModel.DataAnnotations;

namespace MyJwtAuthService.Requests
{
    public class ForgotPasswordRequest
    {
        public required string Email { get; set; }
    }
}
