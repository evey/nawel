using FluentAssertions;
using Nawel.Api.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Nawel.Api.Tests.Integration;

public class GiftReservationFlowTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public GiftReservationFlowTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
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
    public async Task GiftFlow_CreateGift_RequiresAuthentication()
    {
        // Arrange - No authentication
        var createGiftDto = new CreateGiftDto
        {
            Name = "New Gift",
            Description = "Test description"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/gifts", createGiftDto);

        // Assert - Should be unauthorized
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GiftFlow_CreateAndRetrieveGift_Success()
    {
        // Arrange - Login as user
        var token = await GetAuthTokenAsync("user", "user123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createGiftDto = new CreateGiftDto
        {
            Name = "Integration Test Gift",
            Description = "Created in integration test",
            Price = 49.99m,
            Url = "https://example.com/gift"
        };

        // Act - Create gift
        var createResponse = await _client.PostAsJsonAsync("/api/gifts", createGiftDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdGift = await createResponse.Content.ReadFromJsonAsync<GiftDto>();
        createdGift.Should().NotBeNull();
        createdGift!.Name.Should().Be("Integration Test Gift");

        // Act - Retrieve user's gifts
        var year = DateTime.UtcNow.Year;
        var getResponse = await _client.GetAsync($"/api/gifts/my-gifts/{year}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var gifts = await getResponse.Content.ReadFromJsonAsync<List<GiftDto>>();
        gifts.Should().NotBeNull();
        gifts.Should().Contain(g => g.Name == "Integration Test Gift");
    }

    [Fact]
    public async Task GiftFlow_ReserveGift_RequiresDifferentUser()
    {
        // Arrange - Login as user (owner of gift)
        var ownerToken = await GetAuthTokenAsync("user", "user123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ownerToken);

        // Try to reserve own gift (gift ID 1 belongs to user ID 2)
        var reserveDto = new ReserveGiftDto { Comment = "Test reservation" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/gifts/1/reserve", reserveDto);

        // Assert - Should fail (can't reserve own gift)
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GiftFlow_ReserveGift_AsOtherUser_Success()
    {
        // Arrange - Login as admin (different user)
        var adminToken = await GetAuthTokenAsync("admin", "admin123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var reserveDto = new ReserveGiftDto { Comment = "Reserved by admin" };

        // Act - Reserve gift that belongs to regular user
        var response = await _client.PostAsJsonAsync("/api/gifts/1/reserve", reserveDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var reservedGift = await response.Content.ReadFromJsonAsync<GiftDto>();
        reservedGift.Should().NotBeNull();
        reservedGift!.IsTaken.Should().BeTrue();
        reservedGift.TakenByUserId.Should().Be(1); // Admin user ID
        reservedGift.Comment.Should().Be("Reserved by admin");
    }

    [Fact]
    public async Task GiftFlow_UnreserveGift_Success()
    {
        // Arrange - First reserve a gift
        var adminToken = await GetAuthTokenAsync("admin", "admin123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var reserveDto = new ReserveGiftDto { Comment = "To be unreserved" };
        await _client.PostAsJsonAsync("/api/gifts/1/reserve", reserveDto);

        // Act - Unreserve the gift
        var unreserveResponse = await _client.PostAsync("/api/gifts/1/unreserve", null);

        // Assert
        unreserveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var unreservedGift = await unreserveResponse.Content.ReadFromJsonAsync<GiftDto>();
        unreservedGift.Should().NotBeNull();
        unreservedGift!.IsTaken.Should().BeFalse();
        unreservedGift.TakenByUserId.Should().BeNull();
    }

    [Fact]
    public async Task GiftFlow_GroupGiftParticipation_Success()
    {
        // Arrange - Login as admin
        var adminToken = await GetAuthTokenAsync("admin", "admin123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act - Participate in group gift (gift ID 2 is a group gift)
        var response = await _client.PostAsync("/api/gifts/2/participate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var gift = await response.Content.ReadFromJsonAsync<GiftDto>();
        gift.Should().NotBeNull();
        gift!.IsGroupGift.Should().BeTrue();
        gift.ParticipantCount.Should().BeGreaterThan(0);
        gift.ParticipantNames.Should().Contain("Admin");
    }

    [Fact]
    public async Task GiftFlow_UpdateGift_OnlyOwnerCanUpdate()
    {
        // Arrange - Login as admin (not owner)
        var adminToken = await GetAuthTokenAsync("admin", "admin123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var updateDto = new UpdateGiftDto
        {
            Name = "Hacked name"
        };

        // Act - Try to update gift that doesn't belong to admin
        var response = await _client.PutAsJsonAsync("/api/gifts/1", updateDto);

        // Assert - Should fail (not owner)
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GiftFlow_DeleteGift_OnlyOwnerCanDelete()
    {
        // Arrange - Login as admin (not owner)
        var adminToken = await GetAuthTokenAsync("admin", "admin123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act - Try to delete gift that doesn't belong to admin
        var response = await _client.DeleteAsync("/api/gifts/1");

        // Assert - Should fail (not owner)
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GiftFlow_GetOtherUsersGifts_Success()
    {
        // Arrange - Login as admin
        var adminToken = await GetAuthTokenAsync("admin", "admin123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act - Get gifts of user ID 2
        var year = DateTime.UtcNow.Year;
        var response = await _client.GetAsync($"/api/gifts/user/2/{year}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var gifts = await response.Content.ReadFromJsonAsync<List<GiftDto>>();
        gifts.Should().NotBeNull();
        gifts.Should().NotBeEmpty();
    }
}
