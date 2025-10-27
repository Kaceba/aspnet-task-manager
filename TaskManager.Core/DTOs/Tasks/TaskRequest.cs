using TaskManager.Core.Enums;

namespace TaskManager.Core.DTOs.Tasks;

public class TaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Priority Priority { get; set; } = Priority.Medium;
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public DateTime? DueDate { get; set; }
    public int? CategoryId { get; set; }
    public List<string> Tags { get; set; } = new();
}
