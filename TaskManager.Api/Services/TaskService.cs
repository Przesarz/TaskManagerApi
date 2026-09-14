using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs;
using TaskManager.Api.Enums;
using TaskManager.Api.Models;

namespace TaskManager.Api.Services
{
    public class TaskService : ITaskService
    {
        private readonly TaskManagerDbContext _context;
        private readonly ILogger _logger;

        public TaskService(TaskManagerDbContext context, ILogger<TaskService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TaskPagedResultDto> GetTasks(TaskFilterDto filter, int userId)
        {
            var query = _context.Tasks.AsQueryable();
            query = query.Where(t=>t.UserId == userId);

            if (filter.IsCompleted.HasValue)
            {
                query = query.Where(t => t.IsCompleted == filter.IsCompleted.Value);
            }

            if (filter.Priority.HasValue)
            {
                query = query.Where(t => t.Priority == filter.Priority.Value);
            }
            
            if(!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(t => t.Title.Contains(filter.Search));
            }

            switch(filter.SortBy)
            {
                case SortBy.Title:
                    switch(filter.SortDirection)
                    {
                        case SortDirection.Asc:
                            query = query.OrderBy(t => t.Title);
                            break;

                        case SortDirection.Desc:
                            query = query.OrderByDescending(t => t.Title); 
                            break;
                    }
                break;

                case SortBy.DueDate:
                    switch (filter.SortDirection)
                    {
                        case SortDirection.Asc:
                            query = query.OrderBy(t => t.DueDate);
                            break;

                        case SortDirection.Desc:
                            query = query.OrderByDescending(t => t.DueDate);
                            break;
                    }
                break;
            }

            var totalCount = await query.CountAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

            query = query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize);

            var items = await query.ToListAsync();

            var itemDtos = items.Select(task => new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                DueDate = task.DueDate,
                Priority = task.Priority
            }).ToList();

            var result = new TaskPagedResultDto
            {
                Items = itemDtos,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            return result;
        }

        public async Task<TaskResponseDto?> GetTaskById(int id, int userId)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if(task == null)
            {
                _logger.LogWarning(
                "Nie znaleziono zadania o id: {TaskId}",
                id);
                return null;
            }
            else
            {
                var taskDto = new TaskResponseDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    IsCompleted = task.IsCompleted,
                    CreatedAt = task.CreatedAt,
                    DueDate = task.DueDate,
                    Priority = task.Priority
                };
                _logger.LogInformation(
                "Pobrano zadanie o id: {TaskId}",
                id);
                return taskDto;
            }
        }

        public async Task<TaskResponseDto> CreateTask (CreateTaskDto dto, int userId)
        {
            var newTask = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                DueDate = dto.DueDate,
                Priority = dto.Priority,
                UserId = userId
            };

            _context.Tasks.Add(newTask);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Utworzono zadanie o id: {TaskId}",
                newTask.Id);

            var responseDto = new TaskResponseDto
            {
                Id = newTask.Id,
                Title = newTask.Title,
                Description = newTask.Description,
                IsCompleted = newTask.IsCompleted,
                CreatedAt = newTask.CreatedAt,
                DueDate = newTask.DueDate,
                Priority = newTask.Priority
            };
            
            return responseDto;
        }

        public async Task<TaskResponseDto?> UpdateTask (int id, UpdateTaskDto dto, int userId)
        {
            var taskToUpdate = await _context.Tasks.FirstOrDefaultAsync(t=>t.Id==id && t.UserId == userId);

            if(taskToUpdate==null)
            {
                _logger.LogWarning(
                "Nie znaleziono zadania o id: {TaskId}",
                id);
                return null;
            }

            taskToUpdate.Title = dto.Title;
            taskToUpdate.Description = dto.Description;
            taskToUpdate.IsCompleted = dto.IsCompleted;
            taskToUpdate.DueDate = dto.DueDate;
            taskToUpdate.Priority = dto.Priority;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
            "Zaktualizowano zadanie o id: {TaskId}",
            id);

            var responseDto = new TaskResponseDto
            {
                Id = taskToUpdate.Id,
                Title = taskToUpdate.Title,
                Description = taskToUpdate.Description,
                IsCompleted = taskToUpdate.IsCompleted,
                CreatedAt = taskToUpdate.CreatedAt,
                DueDate = taskToUpdate.DueDate,
                Priority = taskToUpdate.Priority
            };

            return responseDto;
        }

        public async Task<bool> DeleteTask(int id, int userId)
        {
            var taskToDelete = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if(taskToDelete==null)
            {
                _logger.LogWarning(
                "Nie znaleziono zadania o id: {TaskId}",
                id);
                return false;
            }
            _context.Tasks.Remove(taskToDelete);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
            "Usunięto zadanie o id: {TaskId}",
            id);

            return true;
        }
    }
}
