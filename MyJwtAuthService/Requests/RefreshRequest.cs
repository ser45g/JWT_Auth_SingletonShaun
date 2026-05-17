using System.ComponentModel.DataAnnotations;

namespace MyJwtAuthService.Requests
{
    public class RefreshRequest
    {
        public required string RefreshToken { get; set; }
    }
}
