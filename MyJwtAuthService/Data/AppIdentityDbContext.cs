using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyJwtAuthService.Models;

namespace MyJwtAuthService.Data
{
    public class AppIdentityDbContext : IdentityDbContext<ApplicationUser, Role, Guid>{
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options){}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
