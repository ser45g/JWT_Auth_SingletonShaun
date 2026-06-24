using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MyJwtAuthService.Data
{
    public class HangfireDbContext : DbContext{
        public HangfireDbContext(DbContextOptions<HangfireDbContext> options) : base(options){}
       
    }
}
