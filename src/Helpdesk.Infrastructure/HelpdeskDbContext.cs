using Helpdesk.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Infrastructure
{
    public class HelpdeskDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
    {
        public HelpdeskDbContext(DbContextOptions<HelpdeskDbContext> options)
            : base(options)
        {
        }

        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<TicketHistory> TicketHistory => Set<TicketHistory>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Must run first: this is what maps the Identity entity types at all.
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(HelpdeskDbContext).Assembly);

            // AppUser -> "Users" is set in AppUserConfiguration; drop the AspNet
            // prefix on the rest so the whole schema reads consistently.
            builder.Entity<IdentityRole<int>>().ToTable("Roles");
            builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        }
    }
}
