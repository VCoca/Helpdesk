using Helpdesk.Domain.Entities;
using Helpdesk.Domain.Enums;

namespace Helpdesk.Api.Dtos
{
    public record TicketListItemDto(
        int Id, string Title, TicketStatus Status, Priority Priority,
        string CategoryName, string AuthorName, string? AssignedAgentName,
        DateTime CreatedAt);

    public record TicketDetailDto(
        int Id, string Title, TicketStatus Status, Priority Priority,
        string CategoryName, string AuthorName, string? AssignedAgentName,
        DateTime CreatedAt, string Description, DateTime UpdatedAt,
        DateTime? ClosedAt);

    public record CreateTicketDto(string Title, string Description, Priority Priority, int CategoryId);

    public static class TicketMappings
    {
        public static TicketListItemDto ToListItemDto(this Ticket t) => new(
            t.Id,
            t.Title,
            t.Status,
            t.Priority,
            t.Category.Name,
            t.Author.FullName,
            t.AssignedAgent?.FullName,
            t.CreatedAt);

        public static TicketDetailDto ToDetailDto(this Ticket t) => new(
            t.Id,
            t.Title,
            t.Status,
            t.Priority,
            t.Category.Name,
            t.Author.FullName,
            t.AssignedAgent?.FullName,
            t.CreatedAt,
            t.Description,
            t.UpdatedAt,
            t.ClosedAt);
    }
}
