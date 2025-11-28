using Microsoft.EntityFrameworkCore;
using TaskApi.Data;
using TaskApi.DTOs;
using TaskApi.Models;

namespace TaskApi.Service
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;

        public TaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync(int userId)
        {
            return await _context.Tasks
                .Where(t => t.UserId == userId)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status.ToString(),
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<TaskResponseDto?> GetTaskByIdAsync(int userId, int taskId)
        {
            return await _context.Tasks
                .Where(t => t.UserId == userId && t.Id == taskId)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status.ToString(),
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TaskResponseDto> CreateTaskAsync(int userId, CreateTaskDto taskDto)
        {
            var task = new TaskItem
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                Status = Enum.Parse<Models.TaskStatus>(taskDto.Status),
                DueDate = taskDto.DueDate,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt
            };
        }

        public async Task<bool> UpdateTaskAsync(int userId, int taskId, UpdateTaskDto taskDto)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Id == taskId);

            if (task is null)
            {
                return false;
            }

            task.Title = taskDto.Title;
            task.Description = taskDto.Description;
            task.Status = Enum.Parse<Models.TaskStatus>(taskDto.Status);
            task.DueDate = taskDto.DueDate;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTaskAsync(int userId, int taskId)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Id == taskId);

            if (task is null)
            {
                return false;
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
