using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamingPlatform.Data;
using GamingPlatform.DTOs;
using GamingPlatform.Models;

namespace GamingPlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto request)
        {
            await EnsureUsersTableAsync();

            var username = request.Username.Trim();
            var email = request.Email.Trim().ToLower();

            var userExists = await _context.Users.AnyAsync(user =>
                user.Username.ToLower() == username.ToLower() || user.Email.ToLower() == email);

            if (userExists)
            {
                return Conflict(new { message = "Користувач з таким email або ім'ям вже існує" });
            }

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = HashPassword(request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(ToResponse(user));
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request)
        {
            await EnsureUsersTableAsync();

            var login = request.Login.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(user =>
                user.Username.ToLower() == login || user.Email.ToLower() == login);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Невірне ім'я користувача/email або пароль" });
            }

            return Ok(ToResponse(user));
        }

        private async Task EnsureUsersTableAsync()
        {
            await _context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Username VARCHAR(100) NOT NULL,
                    Email VARCHAR(255) NOT NULL,
                    PasswordHash VARCHAR(500) NOT NULL,
                    CreatedAt DATETIME(6) NOT NULL,
                    UNIQUE INDEX IX_Users_Username (Username),
                    UNIQUE INDEX IX_Users_Email (Email)
                );");
        }

        private static AuthResponseDto ToResponse(User user)
        {
            return new AuthResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            };
        }

        private static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[1]);
            var expectedHash = Convert.FromBase64String(parts[2]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}
