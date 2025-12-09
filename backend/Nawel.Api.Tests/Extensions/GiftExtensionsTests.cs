using FluentAssertions;
using Nawel.Api.Extensions;
using Nawel.Api.Models;
using Xunit;

namespace Nawel.Api.Tests.Extensions;

public class GiftExtensionsTests
{
    [Fact]
    public void ToDto_WithFullGiftData_MapsAllProperties()
    {
        // Arrange
        var takenByUser = new User
        {
            Id = 2,
            Login = "takenuser",
            FirstName = "Jane",
            Password = "hashedpassword",
            FamilyId = 1
        };

        var gift = new Gift
        {
            Id = 1,
            Name = "Test Gift",
            Description = "A test gift description",
            Link = "https://example.com/gift",
            Image = "gift.jpg",
            Cost = 49.99m,
            Year = 2025,
            Available = false,
            TakenBy = 2,
            TakenByUser = takenByUser,
            Comment = "This is a comment",
            IsGroupGift = false,
            ListId = 1
        };

        // Act
        var dto = gift.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(1);
        dto.Name.Should().Be("Test Gift");
        dto.Description.Should().Be("A test gift description");
        dto.Url.Should().Be("https://example.com/gift");
        dto.ImageUrl.Should().Be("gift.jpg");
        dto.Price.Should().Be(49.99m);
        dto.Year.Should().Be(2025);
        dto.IsTaken.Should().BeTrue(); // !Available
        dto.TakenByUserId.Should().Be(2);
        dto.TakenByUserName.Should().Be("Jane"); // FirstName
        dto.Comment.Should().Be("This is a comment");
        dto.IsGroupGift.Should().BeFalse();
        dto.ParticipantCount.Should().Be(0);
    }

    [Fact]
    public void ToDto_WithAvailableGift_SetsIsTakenToFalse()
    {
        // Arrange
        var gift = new Gift
        {
            Id = 2,
            Name = "Available Gift",
            Available = true,
            Year = 2025,
            ListId = 1
        };

        // Act
        var dto = gift.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.IsTaken.Should().BeFalse();
        dto.TakenByUserId.Should().BeNull();
        dto.TakenByUserName.Should().BeNull();
    }

    [Fact]
    public void ToDto_WithTakenByUserWithoutFirstName_UsesLogin()
    {
        // Arrange
        var takenByUser = new User
        {
            Id = 3,
            Login = "userlogin",
            FirstName = null,
            Password = "hashedpassword",
            FamilyId = 1
        };

        var gift = new Gift
        {
            Id = 3,
            Name = "Gift with login fallback",
            Available = false,
            TakenBy = 3,
            TakenByUser = takenByUser,
            Year = 2025,
            ListId = 1
        };

        // Act
        var dto = gift.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.TakenByUserName.Should().Be("userlogin");
    }

    [Fact]
    public void ToDto_WithGroupGift_MapsParticipants()
    {
        // Arrange
        var participant1 = new User
        {
            Id = 4,
            Login = "participant1",
            FirstName = "Alice",
            Password = "hashedpassword",
            FamilyId = 1
        };

        var participant2 = new User
        {
            Id = 5,
            Login = "participant2",
            FirstName = null,
            Password = "hashedpassword",
            FamilyId = 1
        };

        var gift = new Gift
        {
            Id = 4,
            Name = "Group Gift",
            IsGroupGift = true,
            Year = 2025,
            ListId = 1,
            Participations = new List<GiftParticipation>
            {
                new() { Id = 1, UserId = 4, User = participant1, GiftId = 4 },
                new() { Id = 2, UserId = 5, User = participant2, GiftId = 4 }
            }
        };

        // Act
        var dto = gift.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.IsGroupGift.Should().BeTrue();
        dto.ParticipantCount.Should().Be(2);
        dto.ParticipantNames.Should().NotBeNull();
        dto.ParticipantNames.Should().HaveCount(2);
        dto.ParticipantNames.Should().Contain("Alice");
        dto.ParticipantNames.Should().Contain("participant2");
    }

    [Fact]
    public void ToDto_WithGroupGiftNoParticipations_SetsZeroCount()
    {
        // Arrange
        var gift = new Gift
        {
            Id = 5,
            Name = "Group Gift No Participants",
            IsGroupGift = true,
            Year = 2025,
            ListId = 1,
            Participations = null
        };

        // Act
        var dto = gift.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.IsGroupGift.Should().BeTrue();
        dto.ParticipantCount.Should().Be(0);
        dto.ParticipantNames.Should().BeNull();
    }

    [Fact]
    public void ToDto_WithMinimalGiftData_MapsRequiredProperties()
    {
        // Arrange
        var gift = new Gift
        {
            Id = 6,
            Name = "Minimal Gift",
            Year = 2025,
            ListId = 1
        };

        // Act
        var dto = gift.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(6);
        dto.Name.Should().Be("Minimal Gift");
        dto.Year.Should().Be(2025);
        dto.Description.Should().BeNull();
        dto.Url.Should().BeNull();
        dto.ImageUrl.Should().BeNull();
        dto.Price.Should().BeNull();
        dto.IsTaken.Should().BeFalse(); // Default Available is true
        dto.TakenByUserId.Should().BeNull();
        dto.TakenByUserName.Should().BeNull();
        dto.Comment.Should().BeNull();
        dto.IsGroupGift.Should().BeFalse();
        dto.ParticipantCount.Should().Be(0);
    }

    [Fact]
    public void ToDto_WithNullTakenByUser_ReturnsNullTakenByUserName()
    {
        // Arrange
        var gift = new Gift
        {
            Id = 7,
            Name = "Gift with null user",
            Available = false,
            TakenBy = 99,
            TakenByUser = null, // User not loaded
            Year = 2025,
            ListId = 1
        };

        // Act
        var dto = gift.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.IsTaken.Should().BeTrue();
        dto.TakenByUserId.Should().Be(99);
        dto.TakenByUserName.Should().BeNull();
    }

    [Fact]
    public void ToDto_WithParticipantWithNullUser_UsesUnknown()
    {
        // Arrange
        var gift = new Gift
        {
            Id = 8,
            Name = "Gift with null participant user",
            IsGroupGift = true,
            Year = 2025,
            ListId = 1,
            Participations = new List<GiftParticipation>
            {
                new() { Id = 1, UserId = 99, User = null, GiftId = 8 }
            }
        };

        // Act
        var dto = gift.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.ParticipantCount.Should().Be(1);
        dto.ParticipantNames.Should().NotBeNull();
        dto.ParticipantNames.Should().Contain("Unknown");
    }

    [Fact]
    public void ToDto_MapsLinkToUrl()
    {
        // Arrange
        var gift = new Gift
        {
            Id = 9,
            Name = "Gift with link",
            Link = "https://example.com/product",
            Year = 2025,
            ListId = 1
        };

        // Act
        var dto = gift.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Url.Should().Be("https://example.com/product");
    }

    [Fact]
    public void ToDto_MapsImageToImageUrl()
    {
        // Arrange
        var gift = new Gift
        {
            Id = 10,
            Name = "Gift with image",
            Image = "product-image.jpg",
            Year = 2025,
            ListId = 1
        };

        // Act
        var dto = gift.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.ImageUrl.Should().Be("product-image.jpg");
    }

    [Fact]
    public void ToDto_MapsCostToPrice()
    {
        // Arrange
        var gift = new Gift
        {
            Id = 11,
            Name = "Gift with cost",
            Cost = 99.99m,
            Year = 2025,
            ListId = 1
        };

        // Act
        var dto = gift.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Price.Should().Be(99.99m);
    }
}
