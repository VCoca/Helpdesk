using Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helpdesk.Infrastructure.Configurations
{
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> b)
        {
            b.ToTable("Tickets");

            b.HasKey(t => t.Id);

            b.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            b.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(2000);

            b.Property(t => t.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            b.Property(t => t.Priority)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            b.Property(t => t.CreatedAt).IsRequired();
            b.Property(t => t.UpdatedAt).IsRequired();

            b.HasOne(t => t.Category)
                .WithMany(c => c.Tickets)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Two FKs to Users, so both navigations must be paired explicitly.
            b.HasOne(t => t.Author)
                .WithMany(u => u.CreatedTickets)
                .HasForeignKey(t => t.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Nullable FK: removing an agent unassigns their tickets rather than blocking.
            b.HasOne(t => t.AssignedAgent)
                .WithMany(u => u.AssignedTickets)
                .HasForeignKey(t => t.AssignedAgentId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(t => t.Status)
                .HasDatabaseName("ix_tickets_status");

            b.HasIndex(t => t.Priority)
                .HasDatabaseName("ix_tickets_priority");

            b.HasIndex(t => t.CategoryId)
                .HasDatabaseName("ix_tickets_category");

            b.HasIndex(t => t.AssignedAgentId)
                .HasDatabaseName("ix_tickets_assigned_agent");

            b.HasIndex(t => t.CreatedAt)
                .HasDatabaseName("ix_tickets_created_at");

            // Covers the User list query: own tickets, newest first.
            // Also serves lookups on AuthorId alone, so no separate index for it.
            b.HasIndex(t => new { t.AuthorId, t.CreatedAt })
                .HasDatabaseName("ix_tickets_author_created");
        }
    }
}
