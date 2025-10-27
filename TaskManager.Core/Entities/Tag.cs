namespace TaskManager.Core.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    // Navigation property
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
