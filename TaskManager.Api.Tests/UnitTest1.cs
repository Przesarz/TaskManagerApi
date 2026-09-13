using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs;
using TaskManager.Api.Enums;
using TaskManager.Api.Models;
using TaskManager.Api.Services;

namespace TaskManager.Api.Tests
{
    public class UnitTest1
    {
        private TaskManagerDbContext _context;
        private TaskService _service;

        public UnitTest1()
        {
            var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            _context = new TaskManagerDbContext(options);

            var loggerFactory = LoggerFactory.Create(builder => { });
            var logger = loggerFactory.CreateLogger<TaskService>();

            _service = new TaskService(_context, logger);
        }

        [Fact]
        public async Task GetTaskById_WhenTaskExists_ReturnsTask()
        {
            // Arrange
            var task = new TaskItem
            {
                Title = "Test task",
                Description = "Opis testowy",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                Priority = Priority.High
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var result = await _service.GetTaskById(task.Id);

            Assert.NotNull(result);
            Assert.Equal("Test task", result.Title);
            Assert.Equal("Opis testowy", result.Description);
            Assert.False(result.IsCompleted);
            Assert.Equal(Priority.High, result.Priority);
            Assert.Equal(task.Id, result.Id);
        }

        [Fact]
        public async Task GetTaskById_WhenTaskDoesNotExist_ReturnsNull()
        {
            // Act
            var result = await _service.GetTaskById(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateTask_WhenDataCorrect_ReturnsCreatedTask()
        {
            var createTaskDto = new CreateTaskDto
            {
                Title = "Nowy task",
                Description = "Moj opis",
                DueDate = DateTime.UtcNow,
                Priority = Priority.Medium
            };

            var task = await _service.CreateTask(createTaskDto);
            var savedTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == task.Id);

            Assert.NotNull(task);
            Assert.NotEqual(0, task.Id);
            Assert.Equal("Nowy task", task.Title);
            Assert.Equal("Moj opis", task.Description);
            Assert.Equal(Priority.Medium, task.Priority);
            Assert.False(task.IsCompleted);

            Assert.NotNull(savedTask);
            Assert.Equal("Nowy task", savedTask.Title);
            Assert.Equal(Priority.Medium, savedTask.Priority);
        }

        [Fact]
        public async Task UpdateTask_WhenTaskExists_UpdatesTask()
        {
            // Arrange
            var existingTask = new TaskItem
            {
                Title = "Stary tytu³",
                Description = "Stary opis",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                Priority = Priority.Low
            };

            _context.Tasks.Add(existingTask);
            await _context.SaveChangesAsync();

            var updateDto = new UpdateTaskDto
            {
                Title = "Nowy tytu³",
                Description = "Nowy opis",
                IsCompleted = true,
                DueDate = DateTime.UtcNow.AddDays(7),
                Priority = Priority.High
            };

            var result = await _service.UpdateTask(existingTask.Id, updateDto);
            var savedTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == result.Id);

            Assert.NotNull(result);
            Assert.Equal(updateDto.Title, result.Title);
            Assert.Equal(updateDto.Description, result.Description);
            Assert.Equal(updateDto.IsCompleted, result.IsCompleted);
            Assert.Equal(updateDto.Priority, result.Priority);

            Assert.NotNull(savedTask);
            Assert.Equal(updateDto.Title, savedTask.Title);
            Assert.Equal(updateDto.Priority, savedTask.Priority);
            Assert.True(savedTask.IsCompleted);

        }

        [Fact]
        public async Task UpdateTask_WhenTaskDoesNotExist_ReturnsNull()
        {
            var result = await _service.UpdateTask(999, new UpdateTaskDto
            {
                Title = "Nowy tytu³",
                Description = "Nowy opis",
                IsCompleted = true,
                Priority = Priority.High
            });

            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteTask_WhenTaskExists_DeletesTask()
        {
            // Arrange
            var task = new TaskItem
            {
                Title = "Task do usuniêcia",
                Description = "Test",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                Priority = Priority.Low
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var result = await _service.DeleteTask(task.Id);

            Assert.True(result);

            var deletedTask = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == task.Id);

            Assert.Null(deletedTask);
        }

        [Fact]
        public async Task DeleteTask_WhenTaskDoesNotExist_ReturnsFalse()
        {
            // Act
            var result = await _service.DeleteTask(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetTasks_ReturnsAllTasks()
        {
            // Arrange
            var tasks = new List<TaskItem>
    {
        new TaskItem
        {
            Title = "Task 1",
            Description = "Opis 1",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Low
        },
        new TaskItem
        {
            Title = "Task 2",
            Description = "Opis 2",
            IsCompleted = true,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.High
        },
        new TaskItem
        {
            Title = "Task 3",
            Description = "Opis 3",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Medium
        }
    };

            _context.Tasks.AddRange(tasks);
            await _context.SaveChangesAsync();

            var filter = new TaskFilterDto();

            var result = await _service.GetTasks(filter);

            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Items.Count);
            Assert.Equal("Task 1", result.Items[0].Title);
        }

        [Fact]
        public async Task GetTasks_WhenFilteredByCompleted_ReturnsCompletedTasks()
        {
            var tasks = new List<TaskItem>
    {
        new TaskItem
        {
            Title = "Task 1",
            Description = "Opis 1",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Low
        },
        new TaskItem
        {
            Title = "Task 2",
            Description = "Opis 2",
            IsCompleted = true,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.High
        },
        new TaskItem
        {
            Title = "Task 3",
            Description = "Opis 3",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Medium
        }
    };
            _context.Tasks.AddRange(tasks);
            await _context.SaveChangesAsync();

            var filter = new TaskFilterDto { IsCompleted = true };
            var result = await _service.GetTasks(filter);
            Assert.Equal(1, result.TotalCount);
            Assert.Equal(1, result.Items.Count);
            Assert.Equal("Task 2", result.Items[0].Title);
        }

        [Fact]
        public async Task GetTasks_WhenFilteredByPriority_ReturnsMatchingTasks()
        {
            var tasks = new List<TaskItem>
    {
        new TaskItem
        {
            Title = "Task 1",
            Description = "Opis 1",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Low
        },
        new TaskItem
        {
            Title = "Task 2",
            Description = "Opis 2",
            IsCompleted = true,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.High
        },
        new TaskItem
        {
            Title = "Task 3",
            Description = "Opis 3",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Medium
        }
    };

            _context.Tasks.AddRange(tasks);
            await _context.SaveChangesAsync();

            var filter = new TaskFilterDto
            {
                Priority = Priority.High
            };

            var result = await _service.GetTasks(filter);

            Assert.Equal(1, result.TotalCount);
            Assert.Equal(1, result.Items.Count);
            Assert.Equal("Task 2", result.Items[0].Title);
        }

        [Fact]
        public async Task GetTasks_WhenSearchedByTitle_ReturnsMatchingTasks()
        {
            var tasks = new List<TaskItem>
    {
        new TaskItem
        {
            Title = "Task 1",
            Description = "Opis 1",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Low
        },
        new TaskItem
        {
            Title = "Task 2",
            Description = "Opis 2",
            IsCompleted = true,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.High
        },
        new TaskItem
        {
            Title = "Task 3",
            Description = "Opis 3",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Medium
        }
    };

            _context.Tasks.AddRange(tasks);
            await _context.SaveChangesAsync();

            var filter = new TaskFilterDto
            {
                Search = "2"
            };

            var result = await _service.GetTasks(filter);

            Assert.Equal(1, result.TotalCount);
            Assert.Equal(1, result.Items.Count);
            Assert.Equal("Task 2", result.Items[0].Title);
        }

        [Fact]
        public async Task GetTasks_WhenSortedByTitleAscending_ReturnsTasksInOrder()
        {
            var tasks = new List<TaskItem>
    {
        new TaskItem
        {
            Title = "Task C",
            Description = "Opis C",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Low
        },
        new TaskItem
        {
            Title = "Task A",
            Description = "Opis A",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.High
        },
        new TaskItem
        {
            Title = "Task B",
            Description = "Opis B",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Medium
        }
    };

            _context.Tasks.AddRange(tasks);
            await _context.SaveChangesAsync();

            var filter = new TaskFilterDto
            {
                SortBy = SortBy.Title,
                SortDirection = SortDirection.Asc
            };

            var result = await _service.GetTasks(filter);

            Assert.Equal("Task A", result.Items[0].Title);
            Assert.Equal("Task B", result.Items[1].Title);
            Assert.Equal("Task C", result.Items[2].Title);
            Assert.Equal(3, result.TotalCount);

        }

        [Fact]
        public async Task GetTasks_WhenSortedByTitleDescending_ReturnsTasksInOrder()
        {
            var tasks = new List<TaskItem>
    {
        new TaskItem
        {
            Title = "Task C",
            Description = "Opis C",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Low
        },
        new TaskItem
        {
            Title = "Task A",
            Description = "Opis A",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.High
        },
        new TaskItem
        {
            Title = "Task B",
            Description = "Opis B",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Medium
        }
    };

            _context.Tasks.AddRange(tasks);
            await _context.SaveChangesAsync();

            var filter = new TaskFilterDto
            {
                SortBy = SortBy.Title,
                SortDirection = SortDirection.Desc
            };

            var result = await _service.GetTasks(filter);


            Assert.Equal("Task C", result.Items[0].Title);
            Assert.Equal("Task B", result.Items[1].Title);
            Assert.Equal("Task A", result.Items[2].Title);
            Assert.Equal(3, result.TotalCount);
        }

        [Fact]
        public async Task GetTasks_WhenSortedByDueDateAscending_ReturnsTasksInOrder()
        {
            var tasks = new List<TaskItem>
    {
        new TaskItem
        {
            Title = "Task C",
            Description = "Opis C",
            DueDate = new DateTime(2026, 9, 20),
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Low
        },
        new TaskItem
        {
            Title = "Task A",
            Description = "Opis A",
            DueDate = new DateTime(2026, 9, 10),
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.High
        },
        new TaskItem
        {
            Title = "Task B",
            Description = "Opis B",
            DueDate = new DateTime(2026, 9, 15),
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Medium
        }
    };

            _context.Tasks.AddRange(tasks);
            await _context.SaveChangesAsync();

            var filter = new TaskFilterDto
            {
                SortBy = SortBy.DueDate,
                SortDirection = SortDirection.Asc
            };

            var result = await _service.GetTasks(filter);

            Assert.Equal("Task A", result.Items[0].Title);
            Assert.Equal("Task B", result.Items[1].Title);
            Assert.Equal("Task C", result.Items[2].Title);
            Assert.Equal(3, result.TotalCount);
        }

        [Fact]
        public async Task GetTasks_WhenSortedByDueDateDescending_ReturnsTasksInOrder()
        {
            var tasks = new List<TaskItem>
    {
        new TaskItem
        {
            Title = "Task C",
            Description = "Opis C",
            DueDate = new DateTime(2026, 9, 20),
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Low
        },
        new TaskItem
        {
            Title = "Task A",
            Description = "Opis A",
            DueDate = new DateTime(2026, 9, 10),
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.High
        },
        new TaskItem
        {
            Title = "Task B",
            Description = "Opis B",
            DueDate = new DateTime(2026, 9, 15),
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Medium
        }
    };

            _context.Tasks.AddRange(tasks);
            await _context.SaveChangesAsync();

            var filter = new TaskFilterDto
            {
                SortBy = SortBy.DueDate,
                SortDirection = SortDirection.Desc
            };

            var result = await _service.GetTasks(filter);

            Assert.Equal("Task C", result.Items[0].Title);
            Assert.Equal("Task B", result.Items[1].Title);
            Assert.Equal("Task A", result.Items[2].Title);
            Assert.Equal(3, result.TotalCount);
        }

        [Fact]
        public async Task GetTasks_WhenFilteredByCompletedAndPriority_ReturnsMatchingTasks()
        {
            var tasks = new List<TaskItem>
    {
        new TaskItem
        {
            Title = "Task C",
            Description = "Opis C",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Low
        },
        new TaskItem
        {
            Title = "Task A",
            Description = "Opis A",
            IsCompleted = true,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.High
        },
        new TaskItem
        {
            Title = "Task B",
            Description = "Opis B",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Medium
        }
    };

            _context.Tasks.AddRange(tasks);
            await _context.SaveChangesAsync();

            var filter = new TaskFilterDto
            {
                Priority = Priority.High,
                IsCompleted = true
            };

            var result = await _service.GetTasks(filter);

            Assert.Equal(1, result.TotalCount);
            Assert.Equal(1, result.Items.Count);
            Assert.Equal("Task A", result.Items[0].Title);
        }

        [Fact]
        public async Task GetTasks_WhenFilteredSortedAndPaged_ReturnsCorrectTasks()
        {
            var tasks = new List<TaskItem>
    {
        new TaskItem
        {
            Title = "Task A",
            Description = "Opis A",
            IsCompleted = true,
            DueDate = new DateTime(2026, 9, 20),
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Low
        },
        new TaskItem
        {
            Title = "Task B",
            Description = "Opis B",
            IsCompleted = false,
            DueDate = new DateTime(2026, 9, 10),
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.High
        },
        new TaskItem
        {
            Title = "Task C",
            Description = "Opis C",
            IsCompleted = true,
            DueDate = new DateTime(2026, 9, 15),
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Medium
        },
        new TaskItem
        {
            Title = "Task D",
            Description = "Opis D",
            IsCompleted = true,
            DueDate = new DateTime(2026, 9, 5),
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.High
        },
        new TaskItem
        {
            Title = "Task E",
            Description = "Opis E",
            IsCompleted = false,
            DueDate = new DateTime(2026, 9, 1),
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.High
        }
    };
            var filter = new TaskFilterDto
            {
                IsCompleted = true,
                SortBy = SortBy.DueDate,
                SortDirection = SortDirection.Asc,
                Page = 1,
                PageSize = 2
            };

            _context.AddRange(tasks);
            await _context.SaveChangesAsync();

            var result = await _service.GetTasks(filter);

            Assert.Equal(2, result.Items.Count);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(2, result.TotalPages);
            Assert.Equal("Task D", result.Items[0].Title);
            Assert.Equal("Task C", result.Items[1].Title);
        }

        [Fact]
        public async Task GetTasks_WhenRequestingSecondPage_ReturnsRemainingTasks()
        {
            var tasks = new List<TaskItem>
    {
        new TaskItem
        {
            Title = "Task A",
            Description = "Opis A",
            IsCompleted = true,
            DueDate = new DateTime(2026, 9, 20),
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Low
        },
        new TaskItem
        {
            Title = "Task B",
            Description = "Opis B",
            IsCompleted = false,
            DueDate = new DateTime(2026, 9, 10),
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.High
        },
        new TaskItem
        {
            Title = "Task C",
            Description = "Opis C",
            IsCompleted = true,
            DueDate = new DateTime(2026, 9, 15),
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.Medium
        },
        new TaskItem
        {
            Title = "Task D",
            Description = "Opis D",
            IsCompleted = true,
            DueDate = new DateTime(2026, 9, 5),
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.High
        },
        new TaskItem
        {
            Title = "Task E",
            Description = "Opis E",
            IsCompleted = false,
            DueDate = new DateTime(2026, 9, 1),
            CreatedAt = DateTime.UtcNow,
            Priority = Priority.High
        }
    };
            var filter = new TaskFilterDto
            {
                IsCompleted = true,
                SortBy = SortBy.DueDate,
                SortDirection = SortDirection.Asc,
                Page = 2,
                PageSize = 2
            };

            _context.AddRange(tasks);
            await _context.SaveChangesAsync();

            var result = await _service.GetTasks(filter);

            Assert.Equal(1, result.Items.Count);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(2, result.TotalPages);
            Assert.Equal("Task A", result.Items[0].Title);
        }

        [Fact]
        public async Task GetTasks_WhenNoTasksMatchFilter_ReturnsEmptyResult()
        {
            var tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Title = "Task A",
                    Description = "Opis A",
                    IsCompleted = false,
                    DueDate = new DateTime(2026, 9, 20),
                    CreatedAt = DateTime.UtcNow,
                    Priority = Priority.Low
                },
                new TaskItem
                {
                    Title = "Task B",
                    Description = "Opis B",
                    IsCompleted = false,
                    DueDate = new DateTime(2026, 9, 10),
                    CreatedAt = DateTime.UtcNow,
                    Priority = Priority.High
                },
                new TaskItem
                {
                    Title = "Task C",
                    Description = "Opis C",
                    IsCompleted = false,
                    DueDate = new DateTime(2026, 9, 15),
                    CreatedAt = DateTime.UtcNow,
                    Priority = Priority.Medium
                }
            };

            _context.AddRange(tasks);
            await _context.SaveChangesAsync();

            var filter = new TaskFilterDto
            {
                IsCompleted = true
            };

            var result = await _service.GetTasks(filter);

            Assert.Equal(0, result.TotalCount);
            Assert.Equal(0, result.Items.Count);
            Assert.Equal(0, result.TotalPages);
        }

        [Fact]
        public async Task GetTasks_WhenSearchIsCaseInsensitive_ReturnsMatchingTasks()
        {
            var tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Title = "Task A",
                    Description = "Opis A",
                    IsCompleted = false,
                    DueDate = new DateTime(2026, 9, 20),
                    CreatedAt = DateTime.UtcNow,
                    Priority = Priority.Low
                },
                new TaskItem
                {
                    Title = "Task B",
                    Description = "Opis B",
                    IsCompleted = false,
                    DueDate = new DateTime(2026, 9, 10),
                    CreatedAt = DateTime.UtcNow,
                    Priority = Priority.High
                },
                new TaskItem
                {
                    Title = "Task C",
                    Description = "Opis C",
                    IsCompleted = false,
                    DueDate = new DateTime(2026, 9, 15),
                    CreatedAt = DateTime.UtcNow,
                    Priority = Priority.Medium
                }
            };

            _context.AddRange(tasks);
            await _context.SaveChangesAsync();

            var filter = new TaskFilterDto
            {
                Search = "task"
            };

            var result = await _service.GetTasks(filter);

            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Items);
        }


        [Fact]
        public async Task GetTasks_WhenSearchIsEmpty_ReturnsAllTasks()
        {
            var tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Title = "Task A",
                    Description = "Opis A",
                    IsCompleted = false,
                    DueDate = new DateTime(2026, 9, 20),
                    CreatedAt = DateTime.UtcNow,
                    Priority = Priority.Low
                },
                new TaskItem
                {
                    Title = "Task B",
                    Description = "Opis B",
                    IsCompleted = false,
                    DueDate = new DateTime(2026, 9, 10),
                    CreatedAt = DateTime.UtcNow,
                    Priority = Priority.High
                },
                new TaskItem
                {
                    Title = "Task C",
                    Description = "Opis C",
                    IsCompleted = false,
                    DueDate = new DateTime(2026, 9, 15),
                    CreatedAt = DateTime.UtcNow,
                    Priority = Priority.Medium
                }
            };

            var filter = new TaskFilterDto
            {
                Search = ""
            };

            _context.AddRange(tasks);
            await _context.SaveChangesAsync();

            var result = await _service.GetTasks(filter);

            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Items.Count);
        }


        [Fact]
        public async Task GetTasks_WhenSearchIsNull_ReturnsAllTasks()
        {
            var tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Title = "Task A",
                    Description = "Opis A",
                    IsCompleted = false,
                    DueDate = new DateTime(2026, 9, 20),
                    CreatedAt = DateTime.UtcNow,
                    Priority = Priority.Low
                },
                new TaskItem
                {
                    Title = "Task B",
                    Description = "Opis B",
                    IsCompleted = false,
                    DueDate = new DateTime(2026, 9, 10),
                    CreatedAt = DateTime.UtcNow,
                    Priority = Priority.High
                },
                new TaskItem
                {
                    Title = "Task C",
                    Description = "Opis C",
                    IsCompleted = false,
                    DueDate = new DateTime(2026, 9, 15),
                    CreatedAt = DateTime.UtcNow,
                    Priority = Priority.Medium
                }
            };

            var filter = new TaskFilterDto
            {
                Search = null
            };

            _context.AddRange(tasks);
            await _context.SaveChangesAsync();

            var result = await _service.GetTasks(filter);

            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Items.Count);
        }


        [Fact]
        public async Task GetTasks_WhenSearchIsWhitespace_ReturnsAllTasks()
        {
            var tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Title = "Task A",
                    Description = "Opis A",
                    IsCompleted = false,
                    DueDate = new DateTime(2026, 9, 20),
                    CreatedAt = DateTime.UtcNow,
                    Priority = Priority.Low
                },
                new TaskItem
                {
                    Title = "Task B",
                    Description = "Opis B",
                    IsCompleted = false,
                    DueDate = new DateTime(2026, 9, 10),
                    CreatedAt = DateTime.UtcNow,
                    Priority = Priority.High
                },
                new TaskItem
                {
                    Title = "Task C",
                    Description = "Opis C",
                    IsCompleted = false,
                    DueDate = new DateTime(2026, 9, 15),
                    CreatedAt = DateTime.UtcNow,
                    Priority = Priority.Medium
                }
            };

            var filter = new TaskFilterDto
            {
                Search = "    "
            };

            _context.AddRange(tasks);
            await _context.SaveChangesAsync();

            var result = await _service.GetTasks(filter);

            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Items.Count);
        }

        [Fact]
        public void CreateTaskDto_WhenTitleIsEmpty_IsInvalid()
        {
            var dto = new CreateTaskDto
            {
                Title = ""
            };

            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                dto,
                context,
                results,
                true);

            Assert.False(isValid);
            Assert.NotEmpty(results);
        }

        [Fact]
        public void CreateTaskDto_WhenTitleExceedsMaxLength_IsInvalid()
        {
            var dto = new CreateTaskDto
            {
                Title = new string('A', 101)
            };

            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                dto,
                context,
                results,
                true);

            Assert.False(isValid);
            Assert.NotEmpty(results);
        }

        [Fact]
        public void CreateTaskDto_WhenTitleHasMaxLength_IsValid()
        {
            var dto = new CreateTaskDto
            {
                Title = new string('A', 100),
                Priority = Priority.Medium
            };

            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                dto,
                context,
                results,
                true);

            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void CreateTaskDto_WhenDescriptionIsNull_IsValid()
        {
            var dto = new CreateTaskDto
            {
                Title = new string('A', 100),
                Priority = Priority.Medium,
                Description = null
            };

            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                dto,
                context,
                results,
                true);

            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void CreateTaskDto_WhenDescriptionExceedsMaxLength_IsInvalid()
        {
            var dto = new CreateTaskDto
            {
                Title = new string('A', 100),
                Priority = Priority.Medium,
                Description = new string('A', 101)
            };

            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                dto,
                context,
                results,
                true);

            Assert.False(isValid);
            Assert.NotEmpty(results);
        }

        [Fact]
        public void CreateTaskDto_WhenDescriptionHasMaxLength_IsValid()
        {
            var dto = new CreateTaskDto
            {
                Title = new string('A', 100),
                Priority = Priority.Medium,
                Description = new string('A', 100)
            };

            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                dto,
                context,
                results,
                true);

            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void CreateTaskDto_WhenPriorityIsOutOfRange_IsInvalid()
        {
            var dto = new CreateTaskDto
            {
                Title = new string('A', 100),
                Priority = (Priority)0
            };

            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                dto,
                context,
                results,
                true);

            Assert.False(isValid);
            Assert.NotEmpty(results);
        }

        [Fact]
        public void CreateTaskDto_WhenPriorityIsMaxValue_IsValid()
        {
            var dto = new CreateTaskDto
            {
                Title = new string('A', 100),
                Priority = Priority.High
            };

            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                dto,
                context,
                results,
                true);

            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void CreateTaskDto_WhenPriorityIsAboveMaxValue_IsInvalid()
        {
            var dto = new CreateTaskDto
            {
                Title = new string('A', 100),
                Priority = (Priority)4
            };

            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                dto,
                context,
                results,
                true);

            Assert.False(isValid);
            Assert.NotEmpty(results);
        }

        [Fact]
        public void UpdateTaskDto_WhenTitleIsEmpty_IsInvalid()
        {
            var dto = new UpdateTaskDto
            {
                Title = ""
            };

            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                dto,
                context,
                results,
                true);

            Assert.False(isValid);
            Assert.NotEmpty(results);
        }

        [Fact]
        public void UpdateTaskDto_WhenTitleExceedsMaxLength_IsInvalid()
        {
            var dto = new UpdateTaskDto
            {
                Title = new string('A', 101)
            };

            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                dto,
                context,
                results,
                true);

            Assert.False(isValid);
            Assert.NotEmpty(results);
        }

        [Fact]
        public void UpdateTaskDto_WhenTitleHasMaxLength_IsValid()
        {
            var dto = new UpdateTaskDto
            {
                Title = new string('A', 100)
            };

            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                dto,
                context,
                results,
                true);

            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void UpdateTaskDto_WhenDescriptionExceedsMaxLength_IsInvalid()
        {
            var dto = new UpdateTaskDto
            {
                Description = new string('A', 101)
            };

            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                dto,
                context,
                results,
                true);

            Assert.False(isValid);
            Assert.NotEmpty(results);
        }

        [Fact]
        public void UpdateTaskDto_WhenDescriptionHasMaxLength_IsValid()
        {
            var dto = new UpdateTaskDto
            {
                Title = new string('A', 100),
                Description = new string('A', 100)
            };

            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                dto,
                context,
                results,
                true);

            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void UpdateTaskDto_WhenDescriptionIsNull_IsValid()
        {
            var dto = new UpdateTaskDto
            {
                Title = new string('A', 100),
                Description = null
            };

            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                dto,
                context,
                results,
                true);

            Assert.True(isValid);
            Assert.Empty(results);
        }

    }
}