using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.Api.DTOs;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _service;

        public TasksController(ITaskService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks([FromQuery] TaskFilterDto filter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"mamy ID USERA: {userId}");
            return Ok(await _service.GetTasks(filter, int.Parse(userId!)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var task = await _service.GetTaskById(id, int.Parse(userId!));
            if (task == null)
            {
                return NotFound();
            }
            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var createdTask = await _service.CreateTask(dto, int.Parse(userId!));
            return CreatedAtAction(
                nameof(GetTask),
                new {id = createdTask.Id},
                createdTask);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, UpdateTaskDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var updatedTask = await _service.UpdateTask(id, dto, int.Parse(userId!));

            if (updatedTask == null)
            {
                return NotFound();
            }

            return Ok(updatedTask);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(await _service.DeleteTask(id, int.Parse(userId)))
            {
                return NoContent();
            }
            return NotFound();
        }
    }
}
