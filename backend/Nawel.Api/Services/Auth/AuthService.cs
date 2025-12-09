using Microsoft.EntityFrameworkCore;
using Nawel.Api.Data;
using Nawel.Api.Models;
using Nawel.Api.Constants;
using Nawel.Api.Exceptions;
using System.Security.Cryptography;
using System.Text;

namespace Nawel.Api.Services.Auth;

public class AuthService : IAuthService
{
    private readonly NawelDbContext _context;
    private readonly ILogger<AuthService> _logger;

    public AuthService(NawelDbContext context, ILogger<AuthService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> AuthenticateAsync(string login, string password)
    {
        var user = await _context.Users
            .Include(u => u.Family)
            .FirstOrDefaultAsync(u => u.Login == login);

        if (user == null)
            return null;

        // Check if password is still in legacy MD5 format (32 hex chars, not BCrypt)
        if (user.Password.Length == 32 && !user.Password.StartsWith("$2"))
        {
            // Legacy MD5 password detected - user must reset password
            _logger.LogWarning(
                "User {Login} (ID: {UserId}) attempted login with legacy MD5 password. " +
                "Password reset required for security. Throwing LegacyPasswordException.",
                user.Login, user.Id);
            throw new LegacyPasswordException(user.Id, user.Login, user.Email ?? "");
        }

        // Verify BCrypt password
        if (BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            return user;
        }

        return null;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.Family)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;

        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> GenerateResetTokenAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            throw new InvalidOperationException("User not found");

        // Generate a cryptographically secure random token
        var tokenBytes = RandomNumberGenerator.GetBytes(ApplicationConstants.Authentication.ResetTokenLengthBytes);
        var token = Convert.ToBase64String(tokenBytes);

        // Hash the token with SHA256 before storing in database
        var hashedToken = HashToken(token);
        user.ResetToken = hashedToken;
        user.TokenExpiry = DateTime.UtcNow.AddHours(ApplicationConstants.Authentication.PasswordResetTokenExpirationHours);

        await _context.SaveChangesAsync();

        // Return the plain token to be sent via email
        return token;
    }

    public async Task<bool> ValidateResetTokenAsync(string token)
    {
        // Hash the provided token to compare with stored hash
        var hashedToken = HashToken(token);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == hashedToken);
        if (user == null || user.TokenExpiry == null || user.TokenExpiry < DateTime.UtcNow)
            return false;

        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        // Hash the provided token to compare with stored hash
        var hashedToken = HashToken(token);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == hashedToken);
        if (user == null || user.TokenExpiry == null || user.TokenExpiry < DateTime.UtcNow)
            return false;

        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.ResetToken = null;
        user.TokenExpiry = null;

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Hashes a token using SHA256 for secure storage in the database.
    /// This ensures that if the database is compromised, reset tokens cannot be used directly.
    /// </summary>
    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = sha256.ComputeHash(tokenBytes);
        return Convert.ToHexString(hashBytes).ToLower();
    }
}
