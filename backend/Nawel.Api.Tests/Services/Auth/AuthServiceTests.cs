using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Nawel.Api.Data;
using Nawel.Api.Exceptions;
using Nawel.Api.Models;
using Nawel.Api.Services.Auth;
using Xunit;

namespace Nawel.Api.Tests.Services.Auth;

public class AuthServiceTests : IDisposable
{
    private readonly NawelDbContext _context;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Configure in-memory database
        var options = new DbContextOptionsBuilder<NawelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new NawelDbContext(options);
        _loggerMock = new Mock<ILogger<AuthService>>();
        _authService = new AuthService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region AuthenticateAsync Tests

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ReturnsUser()
    {
        // Arrange
        var family = new Family { Id = 1, Name = "Test Family" };
        var password = "testpassword123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Id = 1,
            Login = "testuser",
            Password = hashedPassword,
            Email = "test@example.com",
            FamilyId = 1,
            Family = family
        };

        _context.Families.Add(family);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.AuthenticateAsync("testuser", password);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Login.Should().Be("testuser");
        result.Family.Should().NotBeNull();
        result.Family!.Name.Should().Be("Test Family");
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidLogin_ReturnsNull()
    {
        // Arrange
        var password = "testpassword123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Id = 1,
            Login = "testuser",
            Password = hashedPassword,
            FamilyId = 1
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.AuthenticateAsync("wronguser", password);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidPassword_ReturnsNull()
    {
        // Arrange
        var password = "testpassword123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Id = 1,
            Login = "testuser",
            Password = hashedPassword,
            FamilyId = 1
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.AuthenticateAsync("testuser", "wrongpassword");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_WithLegacyMD5Password_ThrowsLegacyPasswordException()
    {
        // Arrange - MD5 password is 32 hex characters (legacy format)
        // The system should throw LegacyPasswordException to trigger migration flow
        // Note: MD5 detection happens BEFORE password verification
        var family = new Family { Id = 1, Name = "Test Family" };
        var user = new User
        {
            Id = 1,
            Login = "testuser",
            Password = "5f4dcc3b5aa765d61d8327deb882cf99", // MD5 hash (32 hex chars) - any MD5 will do
            Email = "test@example.com",
            FamilyId = 1,
            Family = family
        };

        _context.Families.Add(family);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act & Assert - Should throw LegacyPasswordException for security
        // Password doesn't matter - MD5 detection happens first
        var act = async () => await _authService.AuthenticateAsync("testuser", "anypassword");

        await act.Should().ThrowAsync<LegacyPasswordException>()
            .Where(ex => ex.UserId == 1
                      && ex.Login == "testuser"
                      && ex.Email == "test@example.com");
    }

    [Fact]
    public async Task AuthenticateAsync_WithLegacyMD5Password_LogsWarning()
    {
        // Arrange
        var family = new Family { Id = 1, Name = "Test Family" };
        var user = new User
        {
            Id = 1,
            Login = "testuser",
            Password = "5f4dcc3b5aa765d61d8327deb882cf99", // MD5 hash
            Email = "test@example.com",
            FamilyId = 1,
            Family = family
        };

        _context.Families.Add(family);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        try
        {
            await _authService.AuthenticateAsync("testuser", "anypassword");
        }
        catch (LegacyPasswordException)
        {
            // Expected
        }

        // Assert - Verify warning was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("legacy MD5 password")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var family = new Family { Id = 1, Name = "Test Family" };
        var user = new User
        {
            Id = 1,
            Login = "testuser",
            Password = "hashedpassword",
            FamilyId = 1,
            Family = family
        };

        _context.Families.Add(family);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.GetUserByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Login.Should().Be("testuser");
        result.Family.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _authService.GetUserByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region UpdatePasswordAsync Tests

    [Fact]
    public async Task UpdatePasswordAsync_WithValidUserId_UpdatesPasswordAndReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Login = "testuser",
            Password = "oldhashedpassword",
            FamilyId = 1
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var newPassword = "newpassword123";

        // Act
        var result = await _authService.UpdatePasswordAsync(1, newPassword);

        // Assert
        result.Should().BeTrue();

        var updatedUser = await _context.Users.FindAsync(1);
        updatedUser.Should().NotBeNull();
        BCrypt.Net.BCrypt.Verify(newPassword, updatedUser!.Password).Should().BeTrue();
    }

    [Fact]
    public async Task UpdatePasswordAsync_WithInvalidUserId_ReturnsFalse()
    {
        // Act
        var result = await _authService.UpdatePasswordAsync(999, "newpassword");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GenerateResetTokenAsync Tests

    [Fact]
    public async Task GenerateResetTokenAsync_WithValidEmail_GeneratesTokenAndSetsExpiry()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Login = "testuser",
            Password = "hashedpassword",
            Email = "test@example.com",
            FamilyId = 1
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var token = await _authService.GenerateResetTokenAsync("test@example.com");

        // Assert
        token.Should().NotBeNullOrEmpty();

        var updatedUser = await _context.Users.FindAsync(1);
        updatedUser.Should().NotBeNull();
        updatedUser!.ResetToken.Should().NotBeNullOrEmpty();
        updatedUser.TokenExpiry.Should().NotBeNull();
        updatedUser.TokenExpiry.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task GenerateResetTokenAsync_WithInvalidEmail_ThrowsException()
    {
        // Act & Assert
        var act = async () => await _authService.GenerateResetTokenAsync("invalid@example.com");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found");
    }

    #endregion

    #region ValidateResetTokenAsync Tests

    [Fact]
    public async Task ValidateResetTokenAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Login = "testuser",
            Password = "hashedpassword",
            Email = "test@example.com",
            FamilyId = 1
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = await _authService.GenerateResetTokenAsync("test@example.com");

        // Act
        var result = await _authService.ValidateResetTokenAsync(token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateResetTokenAsync_WithInvalidToken_ReturnsFalse()
    {
        // Act
        var result = await _authService.ValidateResetTokenAsync("invalid-token");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateResetTokenAsync_WithExpiredToken_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Login = "testuser",
            Password = "hashedpassword",
            Email = "test@example.com",
            ResetToken = "hashedtoken",
            TokenExpiry = DateTime.UtcNow.AddHours(-1), // Expired
            FamilyId = 1
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.ValidateResetTokenAsync("sometoken");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ResetPasswordAsync Tests

    [Fact]
    public async Task ResetPasswordAsync_WithValidToken_ResetsPasswordAndClearsToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Login = "testuser",
            Password = "oldhashedpassword",
            Email = "test@example.com",
            FamilyId = 1
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = await _authService.GenerateResetTokenAsync("test@example.com");
        var newPassword = "newpassword123";

        // Act
        var result = await _authService.ResetPasswordAsync(token, newPassword);

        // Assert
        result.Should().BeTrue();

        var updatedUser = await _context.Users.FindAsync(1);
        updatedUser.Should().NotBeNull();
        BCrypt.Net.BCrypt.Verify(newPassword, updatedUser!.Password).Should().BeTrue();
        updatedUser.ResetToken.Should().BeNull();
        updatedUser.TokenExpiry.Should().BeNull();
    }

    [Fact]
    public async Task ResetPasswordAsync_WithInvalidToken_ReturnsFalse()
    {
        // Act
        var result = await _authService.ResetPasswordAsync("invalid-token", "newpassword");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResetPasswordAsync_WithExpiredToken_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Login = "testuser",
            Password = "hashedpassword",
            Email = "test@example.com",
            ResetToken = "hashedtoken",
            TokenExpiry = DateTime.UtcNow.AddHours(-1), // Expired
            FamilyId = 1
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.ResetPasswordAsync("sometoken", "newpassword");

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
