using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Models;

namespace TaskManager.Api.Data
{
    public class TaskManagerDbContext : DbContext
    {
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<User> Users { get; set; }

        public TaskManagerDbContext(DbContextOptions<TaskManagerDbContext> options)
            : base(options)
        {

        }
    }
}
