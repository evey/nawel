using Nawel.Api.Models;

namespace Nawel.Api.Services.Auth;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(string login, string password);
    Task<User?> GetUserByIdAsync(int userId);
    Task<bool> UpdatePasswordAsync(int userId, string newPassword);
    Task<string> GenerateResetTokenAsync(string email);
    Task<bool> ValidateResetTokenAsync(string token);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
}
