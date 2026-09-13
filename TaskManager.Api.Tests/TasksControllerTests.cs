using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Api.Controllers;
using TaskManager.Api.DTOs;
using TaskManager.Api.Enums;
using TaskManager.Api.Models;
using TaskManager.Api.Services;

namespace TaskManager.Api.Tests
{
    public class TasksControllerTests
    {
        private readonly Mock<ITaskService> _serviceMock;
        private readonly TasksController _controller;

        public TasksControllerTests()
        {
            _serviceMock = new Mock<ITaskService>();
            var taskService = _serviceMock.Object;
            _controller = new TasksController(taskService);
        }

        [Fact]
        public async Task Test()
        {
            var expectedTask = new TaskResponseDto
            {
                Id = 1,
                Title = "Moj task"
            };

            _serviceMock.Setup(TaskService => TaskService.GetTaskById(expectedTask.Id)).ReturnsAsync(expectedTask);
            var result = await _controller.GetTask(expectedTask.Id);
            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<TaskResponseDto>(((OkObjectResult)result).Value);
            Assert.Equal(expectedTask, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetTask_WhenTaskDoesNotExist_ReturnsNotFound()
        {

            _serviceMock.Setup(TaskService => TaskService.GetTaskById(999)).ReturnsAsync((TaskResponseDto?)null);

            var result = await _controller.GetTask(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateTask_WhenTaskIsCreated_ReturnsCreated()
        {
            var taskDto = new CreateTaskDto
            {
                Title = "Moj task",
                Priority = Priority.Medium
            };

            var expectedTask = new TaskResponseDto
            {
                Title = taskDto.Title,
                Priority = taskDto.Priority
            };

            _serviceMock.Setup(TaskService => TaskService.CreateTask(taskDto)).ReturnsAsync(expectedTask);

            var result = await _controller.CreateTask(taskDto);

            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(expectedTask, ((CreatedAtActionResult)result).Value);
            Assert.Equal(nameof(TasksController.GetTask), ((CreatedAtActionResult)result).ActionName);
            Assert.Equal(expectedTask.Id, ((CreatedAtActionResult)result).RouteValues["id"]);
        }

        [Fact]
        public async Task UpdateTask_WhenTaskExists_ReturnsOk()
        {

            var postTask = new UpdateTaskDto
            {
                Title = "Moj nowy task"
            };

            var expectedTask = new TaskResponseDto
            {
                Title = postTask.Title,
            };

            _serviceMock.Setup(TaskService => TaskService.UpdateTask(4, postTask)).ReturnsAsync(expectedTask);

            var result = await _controller.UpdateTask(4, postTask);

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedTask, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task UpdateTask_WhenTaskDoesNotExist_ReturnsNotFound()
        {
            var postTask = new UpdateTaskDto
            {
                Title = "Moj nowy task"
            };

            _serviceMock.Setup(TaskService => TaskService.UpdateTask(4, postTask)).ReturnsAsync((TaskResponseDto?)null);

            var result = await _controller.UpdateTask(4, postTask);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteTask_WhenTaskExists_ReturnsNoContent()
        {

            _serviceMock.Setup(TaskService => TaskService.DeleteTask(4)).ReturnsAsync(true);

            var result = await _controller.DeleteTask(4);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTask_WhenTaskDoesNotExist_ReturnsNotFound()
        {

            _serviceMock.Setup(TaskService => TaskService.DeleteTask(4)).ReturnsAsync(false);

            var result = await _controller.DeleteTask(4);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetTasks_ReturnsOk()
        {

            var filter = new TaskFilterDto
            {

            };

            var expectedResponse = new TaskPagedResultDto
            {

            };

            _serviceMock.Setup(TaskService => TaskService.GetTasks(filter)).ReturnsAsync(expectedResponse);


            var result = await _controller.GetTasks(filter);
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, ((OkObjectResult)result).Value);
        }
    }
}
