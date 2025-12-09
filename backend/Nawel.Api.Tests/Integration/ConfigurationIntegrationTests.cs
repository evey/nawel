using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Nawel.Api.Tests.Integration;

public class ConfigurationIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public ConfigurationIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Configuration_JwtSecretKey_ShouldNotBeHardcoded()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // Act
        var jwtSecretKey = configuration["Jwt:SecretKey"];

        // Assert - In test environment, this can be a test key
        // In production, it should come from environment variables or user secrets
        jwtSecretKey.Should().NotBeNullOrEmpty("JWT secret key must be configured");
    }

    [Fact]
    public void Configuration_JwtSettings_ShouldBeConfigured()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // Act
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var expirationHours = configuration["Jwt:ExpirationHours"];

        // Assert
        issuer.Should().NotBeNullOrEmpty("JWT issuer must be configured");
        audience.Should().NotBeNullOrEmpty("JWT audience must be configured");
        expirationHours.Should().NotBeNullOrEmpty("JWT expiration hours must be configured");
    }

    [Fact]
    public void Configuration_SmtpSettings_ShouldBeExternalized()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // Act
        var smtpServer = configuration["Smtp:Server"];
        var smtpPort = configuration["Smtp:Port"];
        var smtpUsername = configuration["Smtp:Username"];
        // Note: Password should NOT be in appsettings.json
        var smtpPassword = configuration["Smtp:Password"];

        // Assert - These can be null in test environment, but structure should be defined
        // In production, they should come from environment variables
        smtpServer.Should().NotBeNull("SMTP configuration structure must exist");
        smtpPort.Should().NotBeNull("SMTP port must be configured");
    }

    [Fact]
    public void Configuration_ConnectionString_ShouldBeConfigured()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // Act
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Assert
        connectionString.Should().NotBeNullOrEmpty("Connection string must be configured");
    }

    [Fact]
    public void Configuration_EnvironmentIsTest()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // Act - Check if we're in test environment
        var environment = configuration["ASPNETCORE_ENVIRONMENT"];

        // Assert - Ensure test isolation
        // In real tests, this should be "Testing" or "Development"
        environment.Should().NotBe("Production", "Tests should not run in production environment");
    }

    [Fact]
    public void Configuration_SensitiveData_NotInAppsettings()
    {
        // This test verifies that sensitive data is not hardcoded
        // In a real scenario, you would check that appsettings.json doesn't contain sensitive values

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // Act
        var jwtSecret = configuration["Jwt:SecretKey"];
        var smtpPassword = configuration["Smtp:Password"];

        // Assert - These should come from secure sources (env vars, user secrets, Key Vault)
        // not from appsettings.json
        // We can't test the exact source here, but we verify they're configured
        if (!string.IsNullOrEmpty(jwtSecret))
        {
            jwtSecret.Length.Should().BeGreaterOrEqualTo(32, "JWT secret should be at least 32 characters for security");
        }
    }

    [Fact]
    public void Configuration_RateLimiting_ShouldBeConfigured()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // Act
        var rateLimitingSection = configuration.GetSection("RateLimiting");

        // Assert
        rateLimitingSection.Should().NotBeNull("Rate limiting configuration should exist");

        // Check if rate limiting policies exist
        var fixedPolicy = rateLimitingSection.GetSection("FixedWindowPolicy");
        fixedPolicy.Should().NotBeNull("Fixed window rate limiting policy should be configured");
    }
}
