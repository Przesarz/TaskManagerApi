using TaskManager.Api.DTOs;
using TaskManager.Api.Models;

namespace TaskManager.Api.Services
{
    public interface ITaskService
    {
        Task<TaskPagedResultDto> GetTasks(TaskFilterDto filter, int userId);

        Task<TaskResponseDto?> GetTaskById(int id, int userId);

        Task<TaskResponseDto> CreateTask(CreateTaskDto dto, int userId);

        Task<TaskResponseDto?> UpdateTask(int id, UpdateTaskDto dto, int userId);

        Task<bool> DeleteTask(int id, int userId);

    }
}
