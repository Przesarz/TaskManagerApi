using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Enums;

namespace TaskManager.Api.Models
{
    [Index(nameof(UserName), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public Role UserRole { get; set; }
    }
}
