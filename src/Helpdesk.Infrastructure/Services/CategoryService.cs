using Helpdesk.Domain.Entities;
using Helpdesk.Infrastructure.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Helpdesk.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly HelpdeskDbContext _db;

        public CategoryService(HelpdeskDbContext db) => _db = db;

        public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Categories
                .AsNoTracking()
                .OrderByDescending(t => t.Name)
                .ToListAsync(ct);
        }
    }
}
