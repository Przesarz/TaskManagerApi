using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskManager.Api.DTOs;
using TaskManager.Api.Enums;


namespace TaskManager.Api.Tests
{
    public class TasksApiTests
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public TasksApiTests()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetTasks_WhenDatabaseIsEmpty_ReturnsEmptyResult()
        {
            var response = await _client.GetAsync("/api/tasks");
            var content = await response.Content.ReadAsStringAsync();

            var tasksReceived = JsonSerializer.Deserialize<TaskPagedResultDto>(content);

            Assert.Equal(0, tasksReceived.TotalCount);

            Console.WriteLine(content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateTask_ReturnsCreated()
        {
            // Arrange
            var postTask = new CreateTaskDto
            {
                Title = "Integration task",
                Description = "Test POST",
                Priority = Priority.High
            };

            
            HttpClient client = _factory.CreateClient();
            

            var result = await client.PostAsJsonAsync("api/tasks", postTask);

            var content = await result.Content.ReadAsStringAsync();

            var taskPosted = JsonSerializer.Deserialize<TaskResponseDto>(content);

            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            Assert.Equal("Integration task", taskPosted.Title);
            Assert.Equal(Priority.High, taskPosted.Priority);

            
            result = await client.GetAsync($"api/tasks/{taskPosted.Id}");
            content = await result.Content.ReadAsStringAsync();
            var getPostedTask = JsonSerializer.Deserialize<TaskResponseDto> (content);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(taskPosted.Title, getPostedTask.Title);

        }

        [Fact]
        public async Task CreateTask_WhenTitleIsEmpty_ReturnsBadRequest()
        {
            var postTask = new CreateTaskDto
            {
                Title = "",
                Priority = Priority.Low
            };

            var result = await _client.PostAsJsonAsync("api/tasks", postTask);

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task DeleteTask_WhenTaskExists_ReturnsNoContentAndTaskIsDeleted()
        {
            var postTask = new CreateTaskDto
            {
                Title = "Moj task",
                Priority = Priority.Low
            };

            var result = await _client.PostAsJsonAsync("api/tasks", postTask);

            Assert.Equal(HttpStatusCode.Created, result.StatusCode);

            var content = await result.Content.ReadAsStringAsync();
            var postedTask = JsonSerializer.Deserialize<TaskResponseDto>(
    content,
    new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    });

            Console.WriteLine(content);
            Console.WriteLine($"ID: {postedTask.Id}");

            var deleteResponse = await _client.DeleteAsync($"api/tasks/{postedTask.Id}");

            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getResponse = await _client.GetAsync($"api/tasks/{postedTask.Id}");

            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}
