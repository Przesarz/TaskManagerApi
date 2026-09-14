using TaskManager.Api.Enums;

namespace TaskManager.Api.DTOs
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public Role Role { get; set; }
    }
}
