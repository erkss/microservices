using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LoginService.Data;
using LoginService.Dtos;
using LoginService.Interfaces;
using LoginService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LoginService.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await _context.Users.AsNoTracking().ToListAsync();
        }

        public async Task<User> RegisterAsync(UserRegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
                throw new ArgumentException("Username cannot be null or empty.");

            if (await _context.Users.AnyAsync(u => u.Username == dto.Username.Trim()))
                throw new InvalidOperationException("User already exists.");

            var role = dto.Role.Trim().ToLower();
            if (role != "user" && role != "admin")
                throw new ArgumentException("Invalid role. Only 'user' or 'admin' is accepted.");

            var user = new User
            {
                Username = dto.Username.Trim(),
                Password = dto.Password?.Trim() ?? string.Empty,
                Role = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<string> LoginAsync(UserLoginDto dto)
        {
            if (dto.Username == null || dto.Password == null)
                throw new ArgumentException("Username and password cannot be null.");

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == dto.Username.Trim() &&
                    u.Password == dto.Password.Trim());

            if (user == null)
                throw new UnauthorizedAccessException("Invalid username or password.");

            return GenerateJwtToken(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) 
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _config["Jwt:Key"];
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("JWT Key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
