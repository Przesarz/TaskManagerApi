namespace TaskManager.Api.DTOs
{
    public class TaskPagedResultDto
    {
        public List<TaskResponseDto> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
