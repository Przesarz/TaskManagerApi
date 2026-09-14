using TaskManager.Api.Enums;

namespace TaskManager.Api.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public Priority Priority { get; set; }
        public int UserId { get; set; }

        public TaskItem() 
        {
        }
    }
}
