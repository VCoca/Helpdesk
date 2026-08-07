using Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helpdesk.Infrastructure.Configurations
{
    public class TicketHistoryConfiguration : IEntityTypeConfiguration<TicketHistory>
    {
        public void Configure(EntityTypeBuilder<TicketHistory> b)
        {
            b.ToTable("TicketHistory");

            b.HasKey(h => h.Id);

            b.Property(h => h.FromStatus)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            b.Property(h => h.ToStatus)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            b.Property(h => h.ChangedAt).IsRequired();

            b.HasOne(h => h.Ticket)
                .WithMany(t => t.History)
                .HasForeignKey(h => h.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(h => h.ChangedBy)
                .WithMany(u => u.HistoryChanges)
                .HasForeignKey(h => h.ChangedById)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(h => new { h.TicketId, h.ChangedAt })
                .HasDatabaseName("ix_history_ticket_changed");
        }
    }
}
