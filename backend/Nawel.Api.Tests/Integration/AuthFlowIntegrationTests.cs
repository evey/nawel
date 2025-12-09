using FluentAssertions;
using Nawel.Api.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Nawel.Api.Tests.Integration;

public class AuthFlowIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthFlowIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task LoginFlow_WithValidCredentials_ReturnsTokenAndUser()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Login = "user",
            Password = "user123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrEmpty();
        loginResponse.User.Should().NotBeNull();
        loginResponse.User.Login.Should().Be("user");
        loginResponse.User.FirstName.Should().Be("Regular");
        loginResponse.User.IsAdmin.Should().BeFalse();
    }

    [Fact]
    public async Task LoginFlow_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Login = "user",
            Password = "wrongpassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginFlow_WithAdminUser_ReturnsAdminFlag()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Login = "admin",
            Password = "admin123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse!.User.IsAdmin.Should().BeTrue();
    }

    [Fact]
    public async Task LoginFlow_WithChildUser_ReturnsChildFlag()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Login = "child",
            Password = "child123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse!.User.IsChildren.Should().BeTrue();
    }

    [Fact]
    public async Task LoginFlow_TokenCanBeUsedForAuthentication()
    {
        // Arrange - First login
        var loginRequest = new LoginRequest
        {
            Login = "user",
            Password = "user123"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // Act - Use token to access protected endpoint
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login!.Token);

        var protectedResponse = await _client.GetAsync($"/api/gifts/my-gifts/{DateTime.UtcNow.Year}");

        // Assert
        protectedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PasswordResetFlow_WithValidEmail_ReturnsSuccess()
    {
        // Arrange
        var request = new ResetPasswordRequestDto
        {
            Email = "user@test.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password-request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PasswordResetFlow_WithInvalidEmail_StillReturnsSuccessForSecurity()
    {
        // Arrange - Don't reveal if email exists
        var request = new ResetPasswordRequestDto
        {
            Email = "nonexistent@test.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password-request", request);

        // Assert - Should return success even if email doesn't exist (security best practice)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task MigrationResetFlow_WithValidLogin_ReturnsSuccess()
    {
        // Arrange
        var request = new RequestMigrationResetDto
        {
            Login = "user"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/request-migration-reset", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("migration");
    }

    [Fact]
    public async Task MigrationResetFlow_WithInvalidLogin_StillReturnsSuccessForSecurity()
    {
        // Arrange - Don't reveal if user exists
        var request = new RequestMigrationResetDto
        {
            Login = "nonexistentuser"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/request-migration-reset", request);

        // Assert - Should return success even if user doesn't exist (security best practice)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
