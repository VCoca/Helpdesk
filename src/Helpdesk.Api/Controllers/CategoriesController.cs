using Helpdesk.Api.Dtos;
using Helpdesk.Domain.Entities;
using Helpdesk.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.Api.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categories;

        public CategoriesController(ICategoryService categories) => _categories = categories;

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TicketListItemDto>>> GetAll(CancellationToken ct)
        {
            var categories = await _categories.GetAllAsync(ct);

            return Ok(categories.Select(c => c.ToCategoryDto()));
        }
    }
}
