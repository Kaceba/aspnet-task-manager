namespace TaskManager.Core.DTOs.Categories;

public class CategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#000000";
}
