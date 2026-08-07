using Helpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helpdesk.Infrastructure.Configurations
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> b)
        {
            b.ToTable("Comments");

            b.HasKey(c => c.Id);

            b.Property(c => c.Text)
                .IsRequired()
                .HasMaxLength(2000);

            b.Property(c => c.CreatedAt).IsRequired();

            b.HasOne(c => c.Ticket)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Comment thread, oldest first.
            b.HasIndex(c => new { c.TicketId, c.CreatedAt })
                .HasDatabaseName("ix_comments_ticket_created");
        }
    }
}
