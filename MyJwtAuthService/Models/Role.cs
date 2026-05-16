using Microsoft.AspNetCore.Identity;

namespace MyJwtAuthService.Models
{
    public class Role:IdentityRole<Guid>
    {
        //public IEnumerable<User> Users { get; set; } = new List<User>();
    }
}
