using TaskManager.Api.DTOs;
using TaskManager.Api.Models;

namespace TaskManager.Api.Services
{
    public interface ITaskService
    {
        Task<TaskPagedResultDto> GetTasks(TaskFilterDto filter);

        Task<TaskResponseDto?> GetTaskById(int id);

        Task<TaskResponseDto> CreateTask(CreateTaskDto dto);

        Task<TaskResponseDto?> UpdateTask(int id, UpdateTaskDto dto);

        Task<bool> DeleteTask(int id);

    }
}
