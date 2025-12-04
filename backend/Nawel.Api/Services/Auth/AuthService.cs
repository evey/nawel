using Microsoft.EntityFrameworkCore;
using Nawel.Api.Data;
using Nawel.Api.Models;
using System.Security.Cryptography;
using System.Text;

namespace Nawel.Api.Services.Auth;

public class AuthService : IAuthService
{
    private readonly NawelDbContext _context;

    public AuthService(NawelDbContext context)
    {
        _context = context;
    }

    public async Task<User?> AuthenticateAsync(string login, string password)
    {
        var user = await _context.Users
            .Include(u => u.Family)
            .FirstOrDefaultAsync(u => u.Login == login);

        if (user == null)
            return null;

        // Check if password is MD5 (old format) or BCrypt (new format)
        if (user.Password.Length == 32 && !user.Password.StartsWith("$2"))
        {
            // Old MD5 password
            var md5Hash = ComputeMd5Hash(password);
            if (user.Password == md5Hash)
            {
                // Valid MD5 password, migrate to BCrypt
                user.Password = BCrypt.Net.BCrypt.HashPassword(password);
                await _context.SaveChangesAsync();
                return user;
            }
        }
        else
        {
            // BCrypt password
            if (BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return user;
            }
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

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        user.ResetToken = token;
        user.TokenExpiry = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour

        await _context.SaveChangesAsync();
        return token;
    }

    public async Task<bool> ValidateResetTokenAsync(string token)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == token);
        if (user == null || user.TokenExpiry == null || user.TokenExpiry < DateTime.UtcNow)
            return false;

        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == token);
        if (user == null || user.TokenExpiry == null || user.TokenExpiry < DateTime.UtcNow)
            return false;

        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.ResetToken = null;
        user.TokenExpiry = null;

        await _context.SaveChangesAsync();
        return true;
    }

    private static string ComputeMd5Hash(string input)
    {
        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = md5.ComputeHash(inputBytes);
        return Convert.ToHexString(hashBytes).ToLower();
    }
}
