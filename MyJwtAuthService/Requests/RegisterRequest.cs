using System.ComponentModel.DataAnnotations;

namespace MyJwtAuthService.Requests
{
    public class RegisterRequest
    {
        public required string Email { get; set; }

        public required string Username { get; set; }

        public  required string Password { get; set; }

        public required string ConfirmPassword { get; set; }
    }
}
