using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskApi.Data;
using TaskApi.DTOs;
using TaskApi.Models;

namespace TaskApi.Service
{
    public class AuthService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            // Check if user already exists
            var existingUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == registerDto.Email);
            if (existingUser != null)
            {
                return null; // Email already exists
            }

            // Create new user
            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Generate JWT token
            return GenerateJwtToken(user);
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // Find user by email
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null; // Invalid credentials, idk about returning null here, maybe a stronger typed system?
            }

            // Generate JWT token
            return GenerateJwtToken(user);
        }

        private AuthResponseDto GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
              new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
              new Claim(JwtRegisteredClaimNames.Email, user.Email),
              new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationInMinutes"]!);
            var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Email = user.Email,
                Expiration = expiration
            };
        }
    }
}
