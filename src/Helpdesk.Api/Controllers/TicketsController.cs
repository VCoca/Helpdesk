using Helpdesk.Api.Dtos;
using Helpdesk.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Helpdesk.Api.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _tickets;

        public TicketsController(ITicketService tickets) => _tickets = tickets;

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TicketListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TicketListItemDto>>> GetAll(CancellationToken ct)
        {
            var tickets = await _tickets.GetAllAsync(ct);

            return Ok(tickets.Select(t => t.ToListItemDto()));
        }

        [HttpGet("{id:int}", Name = nameof(GetById))]
        [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TicketDetailDto>> GetById(int id, CancellationToken ct)
        {
            var ticket = await _tickets.GetByIdAsync(id, ct);

            if (ticket is null)
                return NotFound();

            return Ok(ticket.ToDetailDto());
        }

        [HttpPost]
        [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TicketDetailDto>> Create(
            [FromBody] CreateTicketDto dto,
            CancellationToken ct)
        {
            if (!await _tickets.CategoryExistsAsync(dto.CategoryId, ct))
            {
                ModelState.AddModelError(nameof(dto.CategoryId), "Category does not exist.");
                return ValidationProblem(ModelState);
            }

            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);

            var ticket = await _tickets.CreateAsync(
                dto.Title,
                dto.Description,
                dto.Priority,
                dto.CategoryId,
                userId,
                ct);

            return CreatedAtRoute(
                nameof(GetById),
                new { id = ticket.Id },
                ticket.ToDetailDto());
        }
    }
}
