using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.DTOs;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            return Ok(await _service.GetTasks(filter));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _service.GetTaskById(id);
            if (task == null)
            {
                return NotFound();
            }
            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            var createdTask = await _service.CreateTask(dto);
            return CreatedAtAction(
                nameof(GetTask),
                new {id = createdTask.Id},
                createdTask);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, UpdateTaskDto dto)
        {
            var updatedTask = await _service.UpdateTask(id, dto);

            if (updatedTask == null)
            {
                return NotFound();
            }

            return Ok(updatedTask);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            if(await _service.DeleteTask(id))
            {
                return NoContent();
            }
            return NotFound();
        }
    }
}
