using Helpdesk.Domain.Entities;
using Helpdesk.Domain.Enums;

namespace Helpdesk.Infrastructure.Services.Interfaces
{

    public interface ITicketService
    {
        Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken ct = default);

        Task<Ticket?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<Ticket> CreateAsync(
            string title,
            string description,
            Priority priority,
            int categoryId,
            int authorId,
            CancellationToken ct = default);

        Task<bool> CategoryExistsAsync(int categoryId, CancellationToken ct = default);
    }
}
