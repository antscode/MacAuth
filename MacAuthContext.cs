using MacAuth.DbModels;
using Microsoft.EntityFrameworkCore;

namespace MacAuth
{
    public class MacAuthContext : DbContext
    {
        public MacAuthContext(DbContextOptions<MacAuthContext> options)
            : base(options)
        {
        }

        public DbSet<AuthRequest> AuthRequests { get; set; }

        public DbSet<AuthRequestParam> AuthRequestParams { get; set; }
    }
}

