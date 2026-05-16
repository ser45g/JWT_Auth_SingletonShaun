using Microsoft.EntityFrameworkCore;

namespace MyJwtAuthService.Models
{
    //public class AuthenticationDbContext : DbContext
    //{
    //    public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : base(options){}

    //    protected override void OnModelCreating(ModelBuilder modelBuilder)
    //    {
    //        base.OnModelCreating(modelBuilder);

    //        modelBuilder.Entity<User>(userBuilder =>
    //        {
    //            userBuilder.HasMany<Role>(u=>u.Roles).WithMany(r=>r.Users);
    //        });

    //        modelBuilder.Entity<Role>(roleBuilder =>
    //        {
    //            roleBuilder.HasMany<User>(u => u.Users).WithMany(u => u.Roles);

    //            var roles = Enum.GetNames<RolesEnum>();

    //            roleBuilder.HasData(roles.Select(role => new Role() { Name=role, Id=Guid.NewGuid()}));
    //        });

           
    //    }

    //    public DbSet<User> Users { get; set; }
    //    public DbSet<Role> Roles { get; set; }
    //    public DbSet<RefreshToken> RefreshTokens { get; set; }
    //}


}
