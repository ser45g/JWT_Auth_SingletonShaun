using System.ComponentModel.DataAnnotations;

namespace MyJwtAuthService.Requests
{
    public class LoginRequest
    {
        public required string Username { get; set; }

        public required string Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }
}
