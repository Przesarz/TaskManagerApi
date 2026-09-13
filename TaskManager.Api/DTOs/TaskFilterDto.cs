using TaskManager.Api.Enums;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.DTOs
{
    public class TaskFilterDto
    {
        public bool? IsCompleted { get; set; }
        public Priority? Priority { get; set; }
        public string? Search { get; set; }
        public SortBy? SortBy { get; set; }
        public SortDirection? SortDirection { get; set; }

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;
        [Range(1, 100)]
        public int PageSize { get; set;} = 10;
    }
}
