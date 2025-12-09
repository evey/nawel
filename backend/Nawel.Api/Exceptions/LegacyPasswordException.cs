namespace Nawel.Api.Exceptions;

/// <summary>
/// Exception thrown when a user attempts to login with a legacy MD5 password.
/// This indicates that the user's password must be reset to the new BCrypt format.
/// </summary>
public class LegacyPasswordException : Exception
{
    public string Login { get; }
    public string Email { get; }
    public int UserId { get; }

    public LegacyPasswordException(int userId, string login, string email)
        : base($"Legacy MD5 password detected for user {login}. Password reset required for security.")
    {
        UserId = userId;
        Login = login;
        Email = email;
    }
}
