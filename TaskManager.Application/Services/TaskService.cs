using AutoMapper;
using TaskManager.Application.Interfaces;
using TaskManager.Core.DTOs.Common;
using TaskManager.Core.DTOs.Tasks;
using TaskManager.Core.Entities;
using TaskManager.Core.Enums;
using TaskManager.Core.Interfaces;
using TaskStatus = TaskManager.Core.Enums.TaskStatus;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TaskService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TaskResponse> GetByIdAsync(int id, int userId, CancellationToken cancellationToken = default)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(id, cancellationToken);

        if (task == null || task.IsDeleted)
            throw new KeyNotFoundException($"Task with ID {id} not found");

        if (task.UserId != userId)
            throw new UnauthorizedAccessException("You don't have access to this task");

        return _mapper.Map<TaskResponse>(task);
    }

    public async Task<PagedResult<TaskResponse>> GetAllAsync(int userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var tasks = await _unitOfWork.Tasks.GetByUserIdAsync(userId, cancellationToken);
        var tasksList = tasks.ToList();

        var pagedTasks = tasksList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<TaskResponse>
        {
            Items = _mapper.Map<List<TaskResponse>>(pagedTasks),
            TotalCount = tasksList.Count,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<TaskResponse>> GetByStatusAsync(int userId, TaskStatus status, CancellationToken cancellationToken = default)
    {
        var tasks = await _unitOfWork.Tasks.GetByStatusAsync(userId, status, cancellationToken);
        return _mapper.Map<IEnumerable<TaskResponse>>(tasks);
    }

    public async Task<IEnumerable<TaskResponse>> GetByCategoryAsync(int userId, int categoryId, CancellationToken cancellationToken = default)
    {
        var tasks = await _unitOfWork.Tasks.GetByCategoryAsync(userId, categoryId, cancellationToken);
        return _mapper.Map<IEnumerable<TaskResponse>>(tasks);
    }

    public async Task<IEnumerable<TaskResponse>> GetOverdueTasksAsync(int userId, CancellationToken cancellationToken = default)
    {
        var tasks = await _unitOfWork.Tasks.GetOverdueTasksAsync(userId, cancellationToken);
        return _mapper.Map<IEnumerable<TaskResponse>>(tasks);
    }

    public async Task<TaskResponse> CreateAsync(TaskRequest request, int userId, CancellationToken cancellationToken = default)
    {
        var task = _mapper.Map<TaskItem>(request);
        task.UserId = userId;
        task.CreatedAt = DateTime.UtcNow;

        // Handle tags
        if (request.Tags?.Any() == true)
        {
            foreach (var tagName in request.Tags)
            {
                var existingTag = await _unitOfWork.Tags.GetByNameAsync(tagName, cancellationToken);
                if (existingTag != null)
                {
                    task.Tags.Add(existingTag);
                }
                else
                {
                    task.Tags.Add(new Tag { Name = tagName, CreatedAt = DateTime.UtcNow });
                }
            }
        }

        await _unitOfWork.Tasks.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TaskResponse>(task);
    }

    public async Task<TaskResponse> UpdateAsync(int id, TaskRequest request, int userId, CancellationToken cancellationToken = default)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(id, cancellationToken);

        if (task == null || task.IsDeleted)
            throw new KeyNotFoundException($"Task with ID {id} not found");

        if (task.UserId != userId)
            throw new UnauthorizedAccessException("You don't have access to this task");

        task.Title = request.Title;
        task.Description = request.Description;
        task.Priority = request.Priority;
        task.Status = request.Status;
        task.DueDate = request.DueDate;
        task.CategoryId = request.CategoryId;
        task.UpdatedAt = DateTime.UtcNow;

        // Update tags
        task.Tags.Clear();
        if (request.Tags?.Any() == true)
        {
            foreach (var tagName in request.Tags)
            {
                var existingTag = await _unitOfWork.Tags.GetByNameAsync(tagName, cancellationToken);
                if (existingTag != null)
                {
                    task.Tags.Add(existingTag);
                }
                else
                {
                    task.Tags.Add(new Tag { Name = tagName, CreatedAt = DateTime.UtcNow });
                }
            }
        }

        await _unitOfWork.Tasks.UpdateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TaskResponse>(task);
    }

    public async Task DeleteAsync(int id, int userId, CancellationToken cancellationToken = default)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(id, cancellationToken);

        if (task == null || task.IsDeleted)
            throw new KeyNotFoundException($"Task with ID {id} not found");

        if (task.UserId != userId)
            throw new UnauthorizedAccessException("You don't have access to this task");

        // Soft delete
        task.IsDeleted = true;
        task.DeletedAt = DateTime.UtcNow;

        await _unitOfWork.Tasks.UpdateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
