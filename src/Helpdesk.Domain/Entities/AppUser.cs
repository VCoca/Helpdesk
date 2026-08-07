using Helpdesk.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Helpdesk.Domain.Entities
{
    public class AppUser : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Tickets koje je korisnik prijavio
        public ICollection<Ticket> CreatedTickets { get; set; }
            = new List<Ticket>();

        // Tickets koje je agent preuzeo
        public ICollection<Ticket> AssignedTickets { get; set; }
            = new List<Ticket>();

        public ICollection<Comment> Comments { get; set; }
            = new List<Comment>();
        public ICollection<TicketHistory> HistoryChanges { get; set; }
            = new List<TicketHistory>();
    }
}
