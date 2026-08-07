using Helpdesk.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Helpdesk.Domain.Entities
{
    public class TicketHistory
    {
        public int Id { get; set; }

        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        public int ChangedById { get; set; }
        public AppUser ChangedBy { get; set; } = null!;

        public TicketStatus FromStatus { get; set; }

        public TicketStatus ToStatus { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
