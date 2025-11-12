using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskApi.Controllers;
using TaskApi.Data;
using TaskApi.DTOs;
using TaskApi.Models;

namespace TaskApi.Tests;

public class TasksControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly TasksController _controller;
    private readonly int _testUserId = 1;
    private readonly int _otherUserId = 2;

    public TasksControllerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        // Create test users
        _context.Users.AddRange(
            new User { Id = _testUserId, Email = "test@example.com", PasswordHash = "hash" },
            new User { Id = _otherUserId, Email = "other@example.com", PasswordHash = "hash" }
        );
        _context.SaveChanges();

        _controller = new TasksController(_context);

        // Mock the user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim("sub", _testUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllTasks_WithNoTasks_ShouldReturnEmptyList()
    {
        // Act
        var result = await _controller.GetAllTasks();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var tasks = okResult.Value.Should().BeAssignableTo<IEnumerable<TaskResponseDto>>().Subject;
        tasks.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllTasks_WithUserTasks_ShouldReturnOnlyUserTasks()
    {
        // Arrange
        _context.Tasks.AddRange(
            new TaskItem { Title = "User Task 1", UserId = _testUserId, Status = Models.TaskStatus.Todo },
            new TaskItem { Title = "User Task 2", UserId = _testUserId, Status = Models.TaskStatus.InProgress },
            new TaskItem { Title = "Other User Task", UserId = _otherUserId, Status = Models.TaskStatus.Done }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAllTasks();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var tasks = okResult.Value.Should().BeAssignableTo<IEnumerable<TaskResponseDto>>().Subject.ToList();
        tasks.Should().HaveCount(2);
        tasks.Should().AllSatisfy(t => t.Title.Should().StartWith("User Task"));
    }

    [Fact]
    public async Task GetTaskById_WithExistingTask_ShouldReturnTask()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Test Task",
            Description = "Test Description",
            Status = Models.TaskStatus.Todo,
            UserId = _testUserId
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetTaskById(task.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var taskDto = okResult.Value.Should().BeOfType<TaskResponseDto>().Subject;
        taskDto.Title.Should().Be("Test Task");
        taskDto.Description.Should().Be("Test Description");
        taskDto.Status.Should().Be("Todo");
    }

    [Fact]
    public async Task GetTaskById_WithNonExistentTask_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.GetTaskById(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetTaskById_WithOtherUserTask_ShouldReturnNotFound()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Other User Task",
            UserId = _otherUserId,
            Status = Models.TaskStatus.Todo
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetTaskById(task.Id);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateTask_WithValidData_ShouldCreateAndReturnTask()
    {
        // Arrange
        var createDto = new CreateTaskDto
        {
            Title = "New Task",
            Description = "New Description",
            Status = "Todo",
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        // Act
        var result = await _controller.CreateTask(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var taskDto = createdResult.Value.Should().BeOfType<TaskResponseDto>().Subject;
        taskDto.Title.Should().Be(createDto.Title);
        taskDto.Description.Should().Be(createDto.Description);
        taskDto.Status.Should().Be(createDto.Status);

        // Verify task was saved to database
        var dbTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Title == createDto.Title);
        dbTask.Should().NotBeNull();
        dbTask!.UserId.Should().Be(_testUserId);
    }

    [Fact]
    public async Task CreateTask_ShouldSetCreatedAtToCurrentTime()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow.AddSeconds(-1);
        var createDto = new CreateTaskDto
        {
            Title = "Time Test Task",
            Status = "Todo"
        };

        // Act
        var result = await _controller.CreateTask(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var taskDto = createdResult.Value.Should().BeOfType<TaskResponseDto>().Subject;
        taskDto.CreatedAt.Should().BeAfter(beforeCreate);
        taskDto.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task UpdateTask_WithValidData_ShouldUpdateTask()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Original Title",
            Description = "Original Description",
            Status = Models.TaskStatus.Todo,
            UserId = _testUserId
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateTaskDto
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Status = "InProgress",
            DueDate = DateTime.UtcNow.AddDays(5)
        };

        // Act
        var result = await _controller.UpdateTask(task.Id, updateDto);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        // Verify task was updated in database
        var dbTask = await _context.Tasks.FindAsync(task.Id);
        dbTask.Should().NotBeNull();
        dbTask!.Title.Should().Be("Updated Title");
        dbTask.Description.Should().Be("Updated Description");
        dbTask.Status.Should().Be(Models.TaskStatus.InProgress);
    }

    [Fact]
    public async Task UpdateTask_WithNonExistentTask_ShouldReturnNotFound()
    {
        // Arrange
        var updateDto = new UpdateTaskDto
        {
            Title = "Updated Title",
            Status = "Done"
        };

        // Act
        var result = await _controller.UpdateTask(999, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdateTask_WithOtherUserTask_ShouldReturnNotFound()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Other User Task",
            UserId = _otherUserId,
            Status = Models.TaskStatus.Todo
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateTaskDto
        {
            Title = "Updated Title",
            Status = "Done"
        };

        // Act
        var result = await _controller.UpdateTask(task.Id, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();

        // Verify task was not updated
        var dbTask = await _context.Tasks.FindAsync(task.Id);
        dbTask!.Title.Should().Be("Other User Task");
    }

    [Fact]
    public async Task DeleteTask_WithExistingTask_ShouldDeleteTask()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Task to Delete",
            UserId = _testUserId,
            Status = Models.TaskStatus.Todo
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        var taskId = task.Id;

        // Act
        var result = await _controller.DeleteTask(taskId);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        // Verify task was deleted from database
        var dbTask = await _context.Tasks.FindAsync(taskId);
        dbTask.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTask_WithNonExistentTask_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.DeleteTask(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteTask_WithOtherUserTask_ShouldReturnNotFound()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Other User Task",
            UserId = _otherUserId,
            Status = Models.TaskStatus.Todo
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        var taskId = task.Id;

        // Act
        var result = await _controller.DeleteTask(taskId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();

        // Verify task was not deleted
        var dbTask = await _context.Tasks.FindAsync(taskId);
        dbTask.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateTask_WithAllTaskStatuses_ShouldWork()
    {
        // Arrange & Act & Assert for each status
        var statuses = new[] { "Todo", "InProgress", "Done" };

        foreach (var status in statuses)
        {
            var createDto = new CreateTaskDto
            {
                Title = $"Task with {status} status",
                Status = status
            };

            var result = await _controller.CreateTask(createDto);

            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var taskDto = createdResult.Value.Should().BeOfType<TaskResponseDto>().Subject;
            taskDto.Status.Should().Be(status);
        }

        // Verify all tasks were created
        var allTasks = await _context.Tasks.Where(t => t.UserId == _testUserId).ToListAsync();
        allTasks.Should().HaveCount(3);
    }
}
