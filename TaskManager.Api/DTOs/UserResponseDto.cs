using TaskManager.Api.Enums;

namespace TaskManager.Api.DTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public Role Role {get; set;}
    }
}
