using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskApi.Data;
using TaskApi.DTOs;
using TaskApi.Models;
using TaskApi.Service;

namespace TaskApi.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly AuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        // Setup configuration
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:SecretKey", "ThisIsASecretKeyForTestingPurposesOnly1234567890"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:ExpirationInMinutes", "60"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _authService = new AuthService(_context, _configuration);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUserAndReturnToken()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(registerDto.Email);
        result.Token.Should().NotBeNullOrEmpty();
        result.Expiration.Should().BeAfter(DateTime.UtcNow);

        // Verify user was created in database
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
        user.Should().NotBeNull();
        user!.Email.Should().Be(registerDto.Email);
        BCrypt.Net.BCrypt.Verify(registerDto.Password, user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ShouldReturnNull()
    {
        // Arrange
        var email = "duplicate@example.com";
        var existingUser = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ExistingPassword123!")
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var registerDto = new RegisterDto
        {
            Email = email,
            Password = "NewPassword123!"
        };

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.Should().BeNull();

        // Verify only one user with this email exists
        var userCount = await _context.Users.CountAsync(u => u.Email == email);
        userCount.Should().Be(1);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var email = "login@example.com";
        var password = "Password123!";
        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var loginDto = new LoginDto
        {
            Email = email,
            Password = password
        };

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.Token.Should().NotBeNullOrEmpty();
        result.Expiration.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldReturnNull()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnNull()
    {
        // Arrange
        var email = "user@example.com";
        var correctPassword = "CorrectPassword123!";
        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword)
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var loginDto = new LoginDto
        {
            Email = email,
            Password = "WrongPassword123!"
        };

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_ShouldHashPassword()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "hash@example.com",
            Password = "PlainTextPassword123!"
        };

        // Act
        await _authService.RegisterAsync(registerDto);

        // Assert
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
        user.Should().NotBeNull();
        user!.PasswordHash.Should().NotBe(registerDto.Password); // Password should be hashed
        BCrypt.Net.BCrypt.Verify(registerDto.Password, user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_GeneratedToken_ShouldContainUserClaims()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "claims@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();

        // JWT tokens have 3 parts separated by dots
        var tokenParts = result.Token.Split('.');
        tokenParts.Should().HaveCount(3);
    }

    [Fact]
    public async Task LoginAsync_AfterRegistration_ShouldWork()
    {
        // Arrange
        var email = "fullflow@example.com";
        var password = "Password123!";

        var registerDto = new RegisterDto
        {
            Email = email,
            Password = password
        };

        var loginDto = new LoginDto
        {
            Email = email,
            Password = password
        };

        // Act
        var registerResult = await _authService.RegisterAsync(registerDto);
        var loginResult = await _authService.LoginAsync(loginDto);

        // Assert
        registerResult.Should().NotBeNull();
        loginResult.Should().NotBeNull();
        loginResult!.Email.Should().Be(email);
        loginResult.Token.Should().NotBeNullOrEmpty();
    }
}
