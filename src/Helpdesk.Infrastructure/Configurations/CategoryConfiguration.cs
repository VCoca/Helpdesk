using Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helpdesk.Infrastructure.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> b)
        {
            b.ToTable("Categories");

            b.HasKey(c => c.Id);

            b.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            b.HasIndex(c => c.Name)
                .IsUnique()
                .HasDatabaseName("ix_categories_name");

            // Categories are fixed reference data, not test data: they need no
            // hashing and no ordering, so they belong in the migration itself
            // rather than in DbInitializer. Ids are explicit because HasData
            // requires a stable key to diff against.
            b.HasData(
                new Category { Id = 1, Name = "Hardware" },
                new Category { Id = 2, Name = "Software" },
                new Category { Id = 3, Name = "Network" },
                new Category { Id = 4, Name = "Access" },
                new Category { Id = 5, Name = "Other" });
        }
    }
}
