using Microsoft.EntityFrameworkCore;
using Webhooks.Processing.Models;

namespace Webhooks.Processing.Data
{
    public class WebhooksDbContext : DbContext
    {
        public WebhooksDbContext(DbContextOptions<WebhooksDbContext> options) : base(options)
        {

        }
        public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; }
        public DbSet<WebhookDeliveryAttempt> WebhookDeliveryAttempts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<WebhookSubscription>().ToTable("webhook_subscriptions", "webhooks");

            builder.Entity<WebhookDeliveryAttempt>().ToTable("delivery_attempts", "webhooks");
        }
    }
}