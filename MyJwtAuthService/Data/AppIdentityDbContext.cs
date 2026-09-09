using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyJwtAuthService.Models;
using MyJwtAuthService.Outbox;

namespace MyJwtAuthService.Data
{
    public class AppIdentityDbContext : IdentityDbContext<ApplicationUser, Role, Guid>{
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options){}
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var roleNames = Enum.GetNames<RolesEnum>();
            var roles = roleNames.Select(role => new Role() { Name = role, Id =Guid.NewGuid()});
            optionsBuilder.UseAsyncSeeding(async (context, _, cancellationToken) =>
            {
                if (!await context.Set<Role>().AnyAsync(cancellationToken))
                {
                    await context.Set<Role>().AddRangeAsync(roles, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                }
            }).UseSeeding((context,_) =>
            {
                if (!context.Set<Role>().Any())
                {
                    context.Set<Role>().AddRange(roles);
                    context.SaveChanges();
                }
            });
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RefreshToken>().HasOne(r => r.User).WithMany(u => u.RefreshTokens).IsRequired().OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<OutboxMessage>().HasIndex(x => new { x.OccuredOnUtc, x.ProcessedOnUtc }).HasFilter($"\"{nameof(OutboxMessage.ProcessedOnUtc)}\" IS NULL").IncludeProperties(nameof(OutboxMessage.Id), nameof(OutboxMessage.Content), nameof(OutboxMessage.Type));
        }
    }
}
