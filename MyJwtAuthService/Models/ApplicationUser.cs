using Microsoft.AspNetCore.Identity;

namespace MyJwtAuthService.Models
{
    public class ApplicationUser : IdentityUser<Guid>{
        public IEnumerable<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
