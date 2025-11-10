namespace TaskApi.DTOs
{
    public class UpdateTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = "Todo"; //TODO: maybe use enum later
        public DateTime? DueDate { get; set; }
    }
}
