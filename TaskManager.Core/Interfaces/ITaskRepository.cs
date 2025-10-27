using TaskManager.Core.Entities;
using TaskManager.Core.Enums;
using TaskStatus = TaskManager.Core.Enums.TaskStatus;

namespace TaskManager.Core.Interfaces;

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<IEnumerable<TaskItem>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskItem>> GetByStatusAsync(int userId, TaskStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskItem>> GetByCategoryAsync(int userId, int categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskItem>> GetByPriorityAsync(int userId, Priority priority, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskItem>> GetOverdueTasksAsync(int userId, CancellationToken cancellationToken = default);
}
