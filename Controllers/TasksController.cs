using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskApi.Data;
using TaskApi.DTOs;
using TaskApi.Models;

namespace TaskApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public TasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetAllTasks()
        {
            var userid = GetCurrentUserId();

            var tasks = await _context.Tasks
                .Where(t => t.UserId == userid)
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

            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponseDto>> GetTaskById(int id)
        {
            var userid = GetCurrentUserId();
            var task = await _context.Tasks
                .Where(t => t.UserId == userid && t.Id == id)
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

            if (task is null)
            {
                return NotFound();
            }
            return Ok(task);
        }

        [HttpPost]
        public async Task<ActionResult<TaskResponseDto>> CreateTask(CreateTaskDto taskDto)
        {
            var userid = GetCurrentUserId();
            var task = new TaskItem
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                Status = Enum.Parse<Models.TaskStatus>(taskDto.Status), 
                DueDate = taskDto.DueDate,
                CreatedAt = DateTime.UtcNow,
                UserId = userid
            };
            _context.Tasks.Add(task);

            await _context.SaveChangesAsync();

            var responseDto = new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt
            };

            return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, responseDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto taskDto)
        {
            var userid = GetCurrentUserId();

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.UserId == userid && t.Id == id);

            if (task is null)
            {
                return NotFound();
            }

            task.Title = taskDto.Title;
            task.Description = taskDto.Description;
            task.Status = Enum.Parse<Models.TaskStatus>(taskDto.Status);
            task.DueDate = taskDto.DueDate;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userid = GetCurrentUserId();

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.UserId == userid && t.Id == id);

            if (task is null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
                                ?? User.FindFirst("sub");
            return int.Parse(userIdClaim!.Value);
        }
    }
}
