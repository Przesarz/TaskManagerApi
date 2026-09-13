using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskManager.Api.Data;
using TaskManager.Api.Models;


namespace TaskManager.Api.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection = new SqliteConnection("DataSource=:memory:");
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<TaskManagerDbContext>();

                _connection.Open();

                services.AddDbContext<TaskManagerDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });

                var db = services.BuildServiceProvider().GetRequiredService<TaskManagerDbContext>();

                var entity = db.Model.FindEntityType(typeof(TaskItem));
                var property = entity?.FindProperty(nameof(TaskItem.Description));

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            });
        }
    }
}
