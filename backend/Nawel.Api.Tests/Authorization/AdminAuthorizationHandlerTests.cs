using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Nawel.Api.Authorization;
using Nawel.Api.Data;
using Nawel.Api.Models;
using System.Security.Claims;
using Xunit;

namespace Nawel.Api.Tests.Authorization;

public class AdminAuthorizationHandlerTests : IDisposable
{
    private readonly NawelDbContext _context;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly AdminAuthorizationHandler _handler;
    private readonly ServiceProvider _serviceProvider;

    public AdminAuthorizationHandlerTests()
    {
        // Configure in-memory database
        var options = new DbContextOptionsBuilder<NawelDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new NawelDbContext(options);

        // Setup service collection with DbContext
        var services = new ServiceCollection();
        services.AddScoped(_ => _context);
        _serviceProvider = services.BuildServiceProvider();

        var scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        _serviceScopeFactory = scopeFactory;

        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _handler = new AdminAuthorizationHandler(_httpContextAccessorMock.Object, _serviceScopeFactory);
    }

    public void Dispose()
    {
        try
        {
            _context.Database.EnsureDeleted();
        }
        catch (ObjectDisposedException)
        {
            // Context might already be disposed by the service provider
        }
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task HandleRequirementAsync_WithAdminUser_Succeeds()
    {
        // Arrange
        var adminUser = new User
        {
            Id = 1,
            Login = "adminuser",
            Password = "hashedpassword",
            IsAdmin = true,
            FamilyId = 1
        };

        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var authContext = new AuthorizationHandlerContext(
            new[] { new AdminRequirement() },
            claimsPrincipal,
            null);

        // Act
        await _handler.HandleAsync(authContext);

        // Assert
        authContext.HasSucceeded.Should().BeTrue();
        authContext.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithNonAdminUser_Fails()
    {
        // Arrange
        var regularUser = new User
        {
            Id = 2,
            Login = "regularuser",
            Password = "hashedpassword",
            IsAdmin = false,
            FamilyId = 1
        };

        _context.Users.Add(regularUser);
        await _context.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "2")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var authContext = new AuthorizationHandlerContext(
            new[] { new AdminRequirement() },
            claimsPrincipal,
            null);

        // Act
        await _handler.HandleAsync(authContext);

        // Assert
        authContext.HasSucceeded.Should().BeFalse();
        authContext.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithNonExistentUser_Fails()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "999")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var authContext = new AuthorizationHandlerContext(
            new[] { new AdminRequirement() },
            claimsPrincipal,
            null);

        // Act
        await _handler.HandleAsync(authContext);

        // Assert
        authContext.HasSucceeded.Should().BeFalse();
        authContext.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithNoUserIdClaim_Fails()
    {
        // Arrange
        var claims = new List<Claim>();
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var authContext = new AuthorizationHandlerContext(
            new[] { new AdminRequirement() },
            claimsPrincipal,
            null);

        // Act
        await _handler.HandleAsync(authContext);

        // Assert
        authContext.HasSucceeded.Should().BeFalse();
        authContext.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithInvalidUserIdClaim_Fails()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "invalid")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var authContext = new AuthorizationHandlerContext(
            new[] { new AdminRequirement() },
            claimsPrincipal,
            null);

        // Act
        await _handler.HandleAsync(authContext);

        // Assert
        authContext.HasSucceeded.Should().BeFalse();
        authContext.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithEmptyUserIdClaim_Fails()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var authContext = new AuthorizationHandlerContext(
            new[] { new AdminRequirement() },
            claimsPrincipal,
            null);

        // Act
        await _handler.HandleAsync(authContext);

        // Assert
        authContext.HasSucceeded.Should().BeFalse();
        authContext.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithMultipleAdminUsers_SucceedsForCorrectUser()
    {
        // Arrange
        var adminUser1 = new User
        {
            Id = 10,
            Login = "admin1",
            Password = "hashedpassword",
            IsAdmin = true,
            FamilyId = 1
        };

        var adminUser2 = new User
        {
            Id = 11,
            Login = "admin2",
            Password = "hashedpassword",
            IsAdmin = true,
            FamilyId = 1
        };

        var regularUser = new User
        {
            Id = 12,
            Login = "regular",
            Password = "hashedpassword",
            IsAdmin = false,
            FamilyId = 1
        };

        _context.Users.AddRange(adminUser1, adminUser2, regularUser);
        await _context.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "11")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var authContext = new AuthorizationHandlerContext(
            new[] { new AdminRequirement() },
            claimsPrincipal,
            null);

        // Act
        await _handler.HandleAsync(authContext);

        // Assert
        authContext.HasSucceeded.Should().BeTrue();
        authContext.HasFailed.Should().BeFalse();
    }
}
