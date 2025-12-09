using FluentAssertions;
using Nawel.Api.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Nawel.Api.Tests.Integration;

public class AdminAuthorizationFlowTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AdminAuthorizationFlowTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync(string login, string password)
    {
        var loginRequest = new LoginRequest { Login = login, Password = password };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse!.Token;
    }

    [Fact]
    public async Task AdminFlow_GetAllUsers_RequiresAdminRole()
    {
        // Arrange - Login as regular user (not admin)
        var userToken = await GetAuthTokenAsync("user", "user123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

        // Act
        var response = await _client.GetAsync("/api/admin/users");

        // Assert - Should be forbidden
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminFlow_GetAllUsers_WithAdminRole_Success()
    {
        // Arrange - Login as admin
        var adminToken = await GetAuthTokenAsync("admin", "admin123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        var response = await _client.GetAsync("/api/admin/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        users.Should().NotBeNull();
        users.Should().HaveCountGreaterOrEqualTo(3); // admin, user, child
        users.Should().Contain(u => u.Login == "admin");
        users.Should().Contain(u => u.Login == "user");
        users.Should().Contain(u => u.Login == "child");
    }

    [Fact]
    public async Task AdminFlow_CreateUser_RequiresAdminRole()
    {
        // Arrange - Login as regular user
        var userToken = await GetAuthTokenAsync("user", "user123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

        var createUserDto = new CreateUserDto
        {
            Login = "newuser",
            Password = "password123",
            Email = "newuser@test.com",
            FirstName = "New",
            LastName = "User",
            FamilyId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/users", createUserDto);

        // Assert - Should be forbidden
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminFlow_CreateUser_WithAdminRole_Success()
    {
        // Arrange - Login as admin
        var adminToken = await GetAuthTokenAsync("admin", "admin123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var createUserDto = new CreateUserDto
        {
            Login = "integrationuser",
            Password = "test123",
            Email = "integration@test.com",
            FirstName = "Integration",
            LastName = "Test",
            FamilyId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/users", createUserDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdUser = await response.Content.ReadFromJsonAsync<UserDto>();
        createdUser.Should().NotBeNull();
        createdUser!.Login.Should().Be("integrationuser");
        createdUser.FirstName.Should().Be("Integration");
        createdUser.Email.Should().Be("integration@test.com");
    }

    [Fact]
    public async Task AdminFlow_UpdateUser_RequiresAdminRole()
    {
        // Arrange - Login as regular user
        var userToken = await GetAuthTokenAsync("user", "user123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

        var updateDto = new UpdateUserAdminDto
        {
            FirstName = "Modified"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/admin/users/2", updateDto);

        // Assert - Should be forbidden
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminFlow_UpdateUser_WithAdminRole_Success()
    {
        // Arrange - Login as admin
        var adminToken = await GetAuthTokenAsync("admin", "admin123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var updateDto = new UpdateUserAdminDto
        {
            FirstName = "UpdatedName",
            IsAdmin = false
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/admin/users/2", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedUser = await response.Content.ReadFromJsonAsync<UserDto>();
        updatedUser.Should().NotBeNull();
        updatedUser!.FirstName.Should().Be("UpdatedName");
    }

    [Fact]
    public async Task AdminFlow_DeleteUser_RequiresAdminRole()
    {
        // Arrange - Login as regular user
        var userToken = await GetAuthTokenAsync("user", "user123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

        // Act
        var response = await _client.DeleteAsync("/api/admin/users/3");

        // Assert - Should be forbidden
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminFlow_GetFamilies_RequiresAdminRole()
    {
        // Arrange - Login as regular user
        var userToken = await GetAuthTokenAsync("user", "user123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

        // Act
        var response = await _client.GetAsync("/api/admin/families");

        // Assert - Should be forbidden
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminFlow_GetFamilies_WithAdminRole_Success()
    {
        // Arrange - Login as admin
        var adminToken = await GetAuthTokenAsync("admin", "admin123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        var response = await _client.GetAsync("/api/admin/families");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var families = await response.Content.ReadFromJsonAsync<List<object>>();
        families.Should().NotBeNull();
        families.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AdminFlow_GetStatistics_RequiresAdminRole()
    {
        // Arrange - Login as regular user
        var userToken = await GetAuthTokenAsync("user", "user123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

        // Act
        var response = await _client.GetAsync("/api/admin/statistics");

        // Assert - Should be forbidden
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminFlow_GetStatistics_WithAdminRole_Success()
    {
        // Arrange - Login as admin
        var adminToken = await GetAuthTokenAsync("admin", "admin123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        var response = await _client.GetAsync("/api/admin/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var stats = await response.Content.ReadFromJsonAsync<object>();
        stats.Should().NotBeNull();
    }

    [Fact]
    public async Task AdminFlow_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act - No authentication header
        var response = await _client.GetAsync("/api/admin/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminFlow_CompleteFlow_CreateUpdateDeleteUser()
    {
        // Arrange - Login as admin
        var adminToken = await GetAuthTokenAsync("admin", "admin123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Step 1: Create user
        var createDto = new CreateUserDto
        {
            Login = "tempuser",
            Password = "temp123",
            Email = "temp@test.com",
            FirstName = "Temp",
            LastName = "User",
            FamilyId = 1
        };

        var createResponse = await _client.PostAsJsonAsync("/api/admin/users", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();

        // Step 2: Update user
        var updateDto = new UpdateUserAdminDto
        {
            FirstName = "Modified"
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/admin/users/{createdUser!.Id}", updateDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedUser = await updateResponse.Content.ReadFromJsonAsync<UserDto>();
        updatedUser!.FirstName.Should().Be("Modified");

        // Step 3: Delete user
        var deleteResponse = await _client.DeleteAsync($"/api/admin/users/{createdUser.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 4: Verify user is deleted
        var getResponse = await _client.GetAsync("/api/admin/users");
        var users = await getResponse.Content.ReadFromJsonAsync<List<UserDto>>();
        users.Should().NotContain(u => u.Id == createdUser.Id);
    }
}
