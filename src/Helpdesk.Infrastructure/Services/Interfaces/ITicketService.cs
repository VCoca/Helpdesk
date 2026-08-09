using Helpdesk.Domain.Entities;
using Helpdesk.Domain.Enums;

namespace Helpdesk.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Ticket data access and business logic.
    /// Returns domain entities — DTOs live in Helpdesk.Api, which this project
    /// cannot reference (dependencies run Api -> Infrastructure -> Domain).
    /// The controller maps entities to DTOs.
    /// </summary>
    public interface ITicketService
    {
        /// <summary>Every ticket, newest first. Related Category/Author/AssignedAgent are loaded.</summary>
        Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken ct = default);

        /// <summary>One ticket by id, or null if it does not exist.</summary>
        Task<Ticket?> GetByIdAsync(int id, CancellationToken ct = default);

        /// <summary>
        /// Creates a ticket. Status is always New and the timestamps are set here —
        /// the caller does not get to choose them.
        /// </summary>
        Task<Ticket> CreateAsync(
            string title,
            string description,
            Priority priority,
            int categoryId,
            int authorId,
            CancellationToken ct = default);

        /// <summary>True if the category exists. Used to return 400 instead of a foreign-key 500.</summary>
        Task<bool> CategoryExistsAsync(int categoryId, CancellationToken ct = default);
    }
}
