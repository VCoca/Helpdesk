using Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helpdesk.Infrastructure.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> b)
        {
            b.ToTable("Users");

            b.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(200);

            b.Property(u => u.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            b.Property(u => u.CreatedAt)
                .IsRequired();

            b.HasIndex(u => u.Role)
                .HasDatabaseName("ix_users_role");
        }
    }
}
