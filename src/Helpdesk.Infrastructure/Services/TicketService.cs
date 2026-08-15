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

        private IQueryable<Ticket> WithRelated() =>
            _db.Tickets
                .Include(t => t.Category)
                .Include(t => t.Author)
                .Include(t => t.AssignedAgent)
                .AsNoTracking();

        public async Task<IReadOnlyList<Ticket>> GetAllAsync(int currentUserId, bool isAgent, CancellationToken ct = default)
        {
            var query = WithRelated();

            if (!isAgent)
                query = query.Where(t => t.AuthorId == currentUserId);

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync(ct);
        }

        public async Task<Ticket?> GetByIdAsync(int id, int currentUserId, bool isAgent, CancellationToken ct = default)
        {
            var query = WithRelated();

            if (!isAgent)
                query = query.Where(t => t.AuthorId == currentUserId);

            return await query.FirstOrDefaultAsync(t => t.Id == id, ct);
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

                Status = TicketStatus.New,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync(ct);

            return await WithRelated().FirstOrDefaultAsync(t => t.Id == ticket.Id, ct) ?? ticket;
        }
    }
}
