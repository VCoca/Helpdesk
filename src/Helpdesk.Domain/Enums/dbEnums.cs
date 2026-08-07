using System;
using System.Collections.Generic;
using System.Text;

namespace Helpdesk.Domain.Enums
{
    public enum UserRole { User = 0, Agent = 1}
    public enum TicketStatus { New = 0, InProgress = 1, Resolved = 2, Closed = 3, Rejected = 4 }
    public enum Priority { Low = 0, Medium = 1, High = 2, Critical = 3 }
}
