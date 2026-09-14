using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs;
using TaskManager.Api.Enums;
using TaskManager.Api.Models;

namespace TaskManager.Api.Services
{
    public class AuthService
    {
        private readonly TaskManagerDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthService(TaskManagerDbContext context, PasswordHasher<User> hasher, IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = hasher;
            _configuration = configuration;
        }

        public async Task<UserResponseDto?> Register(RegisterDto dto)
        {
            var newUser = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                UserRole = Role.User,

            };

            var userExist = await _context.Users.AnyAsync(u => u.UserName == newUser.UserName || u.Email == newUser.Email);
            if (userExist) 
            {
                return null;
            }

            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, dto.Password);

            _context.Users.Add(newUser);

            await _context.SaveChangesAsync();

            var responseDto = new UserResponseDto
            {
                Id = newUser.Id,
                UserName = dto.UserName,
                Email = dto.Email,
                Role = Role.User
            };

            return responseDto;
        }

        public async Task<LoginResponseDto?> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == dto.UserName);
            if (user == null)
            {
                return null;
            }

            var passwordVerify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if(passwordVerify == PasswordVerificationResult.Success || passwordVerify == PasswordVerificationResult.SuccessRehashNeeded)
            {
                var jwtKey = _configuration["Jwt:Key"];
                var jwtIssuer = _configuration["Jwt:Issuer"];
                var jwtAudience = _configuration["Jwt:Audience"];
                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey!)
                );
                var credentials = new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256
                );

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, user.UserRole.ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: credentials
                );

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenString = tokenHandler.WriteToken(token);

                return new LoginResponseDto
                {
                    Token = tokenString,
                    UserId = user.Id,
                    UserName = user.UserName,
                    Role = user.UserRole
                };
            }
            else
            {
                return null;
            }
        }
    }
}
