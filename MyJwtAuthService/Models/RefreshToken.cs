
namespace MyJwtAuthService.Models
{
    public class RefreshToken
    {
        public required Guid Id { get; set; }
        public required string Token { get; set; }
        public required Guid UserId { get; set; }
    }
}
