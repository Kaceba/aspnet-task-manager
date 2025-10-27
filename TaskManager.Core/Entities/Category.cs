namespace TaskManager.Core.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#000000";

    // Navigation property
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
