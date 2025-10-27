using TaskManager.Core.DTOs.Common;
using TaskManager.Core.DTOs.Tasks;
using TaskManager.Core.Enums;
using TaskStatus = TaskManager.Core.Enums.TaskStatus;

namespace TaskManager.Application.Interfaces;

public interface ITaskService
{
    Task<TaskResponse> GetByIdAsync(int id, int userId, CancellationToken cancellationToken = default);
    Task<PagedResult<TaskResponse>> GetAllAsync(int userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskResponse>> GetByStatusAsync(int userId, TaskStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskResponse>> GetByCategoryAsync(int userId, int categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskResponse>> GetOverdueTasksAsync(int userId, CancellationToken cancellationToken = default);
    Task<TaskResponse> CreateAsync(TaskRequest request, int userId, CancellationToken cancellationToken = default);
    Task<TaskResponse> UpdateAsync(int id, TaskRequest request, int userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, int userId, CancellationToken cancellationToken = default);
}
