using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Tasks)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Tasks)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.Name.Contains(name))
            .ToListAsync(cancellationToken);
    }
}
