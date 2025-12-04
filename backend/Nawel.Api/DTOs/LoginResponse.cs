namespace Nawel.Api.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Avatar { get; set; } = string.Empty;
    public string? Pseudo { get; set; }
    public bool NotifyListEdit { get; set; }
    public bool NotifyGiftTaken { get; set; }
    public bool DisplayPopup { get; set; }
    public bool IsChildren { get; set; }
    public int FamilyId { get; set; }
    public string? FamilyName { get; set; }
}
