namespace TaskApi.DTOs
{
    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty; //TODO: maybe use enum later
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
