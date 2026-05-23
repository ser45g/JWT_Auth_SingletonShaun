using System.ComponentModel.DataAnnotations;

namespace MyJwtAuthService.Requests
{
    public class ResendRequest
    {
        public required string Email { get; set; }
    }
}
