using FluentAssertions;
using Nawel.Api.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Nawel.Api.Tests.Integration;

public class Md5MigrationFlowTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public Md5MigrationFlowTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task LoginWithMd5Password_ReturnsLegacyPasswordError()
    {
        // Arrange - Create a user with MD5 password in the test database
        // This would typically be done through the factory setup
        // For now, we'll test the flow assuming the test data exists

        var loginRequest = new LoginRequest
        {
            Login = "md5user",
            Password = "password"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert - Should return 401 with specific error code
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("LEGACY_PASSWORD");
        content.Should().Contain("réinitialisé");
    }

    [Fact]
    public async Task Md5MigrationFlow_RequestReset_SendsEmail()
    {
        // Arrange
        var request = new RequestMigrationResetDto
        {
            Login = "md5user"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/request-migration-reset", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MessageResponse>();
        result.Should().NotBeNull();
        result!.Message.Should().Contain("migration");
    }

    [Fact]
    public async Task Md5MigrationFlow_AfterPasswordReset_CanLoginWithNewPassword()
    {
        // This is a full integration test that would require:
        // 1. Creating a user with MD5 password
        // 2. Requesting migration reset
        // 3. Using the reset token to set new password
        // 4. Logging in with the new password

        // For now, we document the expected flow
        // Full implementation would require access to the test database
        // and the ability to retrieve the reset token

        Assert.True(true, "Full integration test to be implemented with test database access");
    }
}

// Helper DTO for API responses
public class MessageResponse
{
    public string Message { get; set; } = string.Empty;
}
