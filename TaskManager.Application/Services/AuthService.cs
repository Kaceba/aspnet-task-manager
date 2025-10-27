using AutoMapper;
using TaskManager.Application.Interfaces;
using TaskManager.Core.DTOs.Auth;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AuthService(IUnitOfWork unitOfWork, ITokenService tokenService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Check if username already exists
        if (await _unitOfWork.Users.UsernameExistsAsync(request.Username, cancellationToken))
        {
            throw new InvalidOperationException("Username already exists");
        }

        // Check if email already exists
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Create user
        var user = _mapper.Map<User>(request);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate token
        var token = _tokenService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        var response = _mapper.Map<AuthResponse>(user);
        response.Token = token;
        response.ExpiresAt = expiresAt;

        return response;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // Find user
        var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username, cancellationToken);
        if (user == null || user.IsDeleted)
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        // Generate token
        var token = _tokenService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        var response = _mapper.Map<AuthResponse>(user);
        response.Token = token;
        response.ExpiresAt = expiresAt;

        return response;
    }
}
