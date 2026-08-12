using Helpdesk.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Helpdesk.Infrastructure.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default);
    }
}
