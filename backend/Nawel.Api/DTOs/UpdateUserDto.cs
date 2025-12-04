using System.ComponentModel.DataAnnotations;

namespace Nawel.Api.DTOs;

public class UpdateUserDto
{
    [EmailAddress]
    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Pseudo { get; set; }

    public bool? NotifyListEdit { get; set; }

    public bool? NotifyGiftTaken { get; set; }

    public bool? DisplayPopup { get; set; }
}

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}
