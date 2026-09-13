using System.ComponentModel.DataAnnotations;
using TaskManager.Api.Enums;

namespace TaskManager.Api.DTOs
{
    public class UpdateTaskDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        [StringLength(100)]
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public Priority Priority { get; set; }
        public bool IsCompleted { get; set; }   
    }
}
