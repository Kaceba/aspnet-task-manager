using TaskApi.DTOs;

namespace TaskApi.Service
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync(int userId);
        Task<TaskResponseDto?> GetTaskByIdAsync(int userId, int taskId);
        Task<TaskResponseDto> CreateTaskAsync(int userId, CreateTaskDto taskDto);
        Task<bool> UpdateTaskAsync(int userId, int taskId, UpdateTaskDto taskDto);
        Task<bool> DeleteTaskAsync(int userId, int taskId);
    }
}
