using TaskManager.Core.Enums;

namespace TaskManager.Core.DTOs.Tasks;

public class TaskResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Priority Priority { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
