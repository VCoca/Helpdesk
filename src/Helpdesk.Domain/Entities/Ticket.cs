using Helpdesk.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Helpdesk.Domain.Entities
{
    public class Ticket
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public TicketStatus Status { get; set; } = TicketStatus.New;

        public Priority Priority { get; set; } = Priority.Medium;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public int AuthorId { get; set; }
        public AppUser Author { get; set; } = null!;

        public int? AssignedAgentId { get; set; }
        public AppUser? AssignedAgent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ClosedAt { get; set; }
        public ICollection<Comment> Comments { get; set; }
        = new List<Comment>();

        public ICollection<TicketHistory> History { get; set; }
            = new List<TicketHistory>();
    }
}
