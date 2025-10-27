using TaskManager.Core.Entities;

namespace TaskManager.Core.Interfaces;

public interface ITagRepository : IRepository<Tag>
{
    Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetTagsByTaskIdAsync(int taskId, CancellationToken cancellationToken = default);
}
