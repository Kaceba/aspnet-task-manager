using TaskManager.Core.Entities;

namespace TaskManager.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
