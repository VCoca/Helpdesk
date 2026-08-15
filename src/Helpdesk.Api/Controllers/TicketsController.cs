using Helpdesk.Api.Dtos;
using Helpdesk.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/tickets")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _tickets;

        public TicketsController(ITicketService tickets) => _tickets = tickets;

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TicketListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<TicketListItemDto>>> GetAll(CancellationToken ct)
        {
            var tickets = await _tickets.GetAllAsync(User.GetUserId(), User.IsAgent(), ct);

            return Ok(tickets.Select(t => t.ToListItemDto()));
        }

        [HttpGet("{id:int}", Name = nameof(GetById))]
        [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TicketDetailDto>> GetById(int id, CancellationToken ct)
        {
            var ticket = await _tickets.GetByIdAsync(id, User.GetUserId(), User.IsAgent(), ct);

            if (ticket is null)
                return NotFound();

            return Ok(ticket.ToDetailDto());
        }

        [HttpPost]
        [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TicketDetailDto>> Create(
            [FromBody] CreateTicketDto dto,
            CancellationToken ct)
        {
            if (!await _tickets.CategoryExistsAsync(dto.CategoryId, ct))
            {
                ModelState.AddModelError(nameof(dto.CategoryId), "Category does not exist.");
                return ValidationProblem(ModelState);
            }

            var ticket = await _tickets.CreateAsync(
                dto.Title,
                dto.Description,
                dto.Priority,
                dto.CategoryId,
                User.GetUserId(),
                ct);

            return CreatedAtRoute(
                nameof(GetById),
                new { id = ticket.Id },
                ticket.ToDetailDto());
        }
    }
}
