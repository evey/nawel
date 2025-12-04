using Nawel.Api.Models;

namespace Nawel.Api.Services.Auth;

public interface IJwtService
{
    string GenerateToken(User user);
    int? ValidateToken(string token);
}
