using System.ComponentModel.DataAnnotations;
using TaskManager.Api.Enums;

namespace TaskManager.Api.DTOs
{
    public class CreateTaskDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        [StringLength(100)]
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        [Range(1, 3)]
        public Priority Priority { get; set; }
    }
}
