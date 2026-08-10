using Helpdesk.Domain.Entities;

namespace Helpdesk.Api.Dtos
{
    public record CategoryDto(int Id, string Name);

    public static class CategoryMappings
    {
        public static CategoryDto ToCategoryDto(this Category c) => new(
            c.Id,
            c.Name);
    }
}
