using System.ComponentModel.DataAnnotations;

namespace MyJwtAuthService.Requests
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Username { get; set; }

        [Required]
        public  required string Password { get; set; }

        [Required]
        public required string ConfirmPassword { get; set; }
    }
}
