using TaskManager.Core.Entities;

namespace TaskManager.Core.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
