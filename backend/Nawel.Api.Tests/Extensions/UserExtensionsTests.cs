using FluentAssertions;
using Nawel.Api.Extensions;
using Nawel.Api.Models;
using Xunit;

namespace Nawel.Api.Tests.Extensions;

public class UserExtensionsTests
{
    [Fact]
    public void ToDto_WithFullUserData_MapsAllProperties()
    {
        // Arrange
        var family = new Family { Id = 1, Name = "Test Family" };
        var user = new User
        {
            Id = 1,
            Login = "testuser",
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Avatar = "avatar.png",
            Pseudo = "Johnny",
            NotifyListEdit = true,
            NotifyGiftTaken = false,
            DisplayPopup = true,
            IsChildren = false,
            IsAdmin = true,
            FamilyId = 1,
            Family = family
        };

        // Act
        var dto = user.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(1);
        dto.Login.Should().Be("testuser");
        dto.Email.Should().Be("test@example.com");
        dto.FirstName.Should().Be("John");
        dto.LastName.Should().Be("Doe");
        dto.Avatar.Should().Be("avatar.png");
        dto.Pseudo.Should().Be("Johnny");
        dto.NotifyListEdit.Should().BeTrue();
        dto.NotifyGiftTaken.Should().BeFalse();
        dto.DisplayPopup.Should().BeTrue();
        dto.IsChildren.Should().BeFalse();
        dto.IsAdmin.Should().BeTrue();
        dto.FamilyId.Should().Be(1);
        dto.FamilyName.Should().Be("Test Family");
    }

    [Fact]
    public void ToDto_WithMinimalUserData_MapsRequiredProperties()
    {
        // Arrange
        var user = new User
        {
            Id = 2,
            Login = "minimaluser",
            Password = "hashedpassword",
            FamilyId = 1
        };

        // Act
        var dto = user.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(2);
        dto.Login.Should().Be("minimaluser");
        dto.Email.Should().BeNull();
        dto.FirstName.Should().BeNull();
        dto.LastName.Should().BeNull();
        dto.Avatar.Should().Be("avatar.png"); // Default value
        dto.Pseudo.Should().BeNull();
        dto.NotifyListEdit.Should().BeFalse();
        dto.NotifyGiftTaken.Should().BeFalse();
        dto.DisplayPopup.Should().BeTrue();
        dto.IsChildren.Should().BeFalse();
        dto.IsAdmin.Should().BeFalse();
        dto.FamilyId.Should().Be(1);
        dto.FamilyName.Should().BeNull();
    }

    [Fact]
    public void ToDto_WithoutFamily_MapsWithNullFamilyName()
    {
        // Arrange
        var user = new User
        {
            Id = 3,
            Login = "nofamilyuser",
            Password = "hashedpassword",
            FamilyId = 1,
            Family = null
        };

        // Act
        var dto = user.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.FamilyId.Should().Be(1);
        dto.FamilyName.Should().BeNull();
    }

    [Fact]
    public void ToDto_WithChildUser_SetsIsChildrenToTrue()
    {
        // Arrange
        var user = new User
        {
            Id = 4,
            Login = "childuser",
            Password = "hashedpassword",
            FamilyId = 1,
            IsChildren = true
        };

        // Act
        var dto = user.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.IsChildren.Should().BeTrue();
    }

    [Fact]
    public void ToDto_WithAdminUser_SetsIsAdminToTrue()
    {
        // Arrange
        var user = new User
        {
            Id = 5,
            Login = "adminuser",
            Password = "hashedpassword",
            FamilyId = 1,
            IsAdmin = true
        };

        // Act
        var dto = user.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.IsAdmin.Should().BeTrue();
    }

    [Fact]
    public void ToDto_WithNotificationSettings_MapsCorrectly()
    {
        // Arrange
        var user = new User
        {
            Id = 6,
            Login = "notifyuser",
            Password = "hashedpassword",
            FamilyId = 1,
            NotifyListEdit = true,
            NotifyGiftTaken = true,
            DisplayPopup = false
        };

        // Act
        var dto = user.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.NotifyListEdit.Should().BeTrue();
        dto.NotifyGiftTaken.Should().BeTrue();
        dto.DisplayPopup.Should().BeFalse();
    }
}
