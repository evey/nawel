using Nawel.Api.DTOs;
using Nawel.Api.Models;

namespace Nawel.Api.Extensions;

public static class UserExtensions
{
    /// <summary>
    /// Convertit un User en UserDto
    /// </summary>
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Login = user.Login,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Avatar = user.Avatar,
            Pseudo = user.Pseudo,
            NotifyListEdit = user.NotifyListEdit,
            NotifyGiftTaken = user.NotifyGiftTaken,
            DisplayPopup = user.DisplayPopup,
            IsChildren = user.IsChildren,
            IsAdmin = user.IsAdmin,
            FamilyId = user.FamilyId,
            FamilyName = user.Family?.Name
        };
    }
}
