using Helpdesk.Domain.Entities;
using Helpdesk.Domain.Enums;
using Helpdesk.Infrastructure.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Infrastructure.Services
{
    public class TicketService : ITicketService
    {
        private readonly HelpdeskDbContext _db;

        public TicketService(HelpdeskDbContext db) => _db = db;

        public async Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Tickets
                .Include(t => t.Category)
                .Include(t => t.Author)
                .Include(t => t.AssignedAgent)
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<Ticket?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Tickets
                .Include(t => t.Category)
                .Include(t => t.Author)
                .Include(t => t.AssignedAgent)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id, ct);
        }

        public async Task<bool> CategoryExistsAsync(int categoryId, CancellationToken ct = default)
        {
            return await _db.Categories.AnyAsync(c => c.Id == categoryId, ct);
        }

        public async Task<Ticket> CreateAsync(
            string title,
            string description,
            Priority priority,
            int categoryId,
            int authorId,
            CancellationToken ct = default)
        {
            var ticket = new Ticket
            {
                Title = title,
                Description = description,
                Priority = priority,
                CategoryId = categoryId,
                AuthorId = authorId,

                // Set on the server
                Status = TicketStatus.New,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync(ct);

            return await GetByIdAsync(ticket.Id, ct) ?? ticket;
        }
    }
}
