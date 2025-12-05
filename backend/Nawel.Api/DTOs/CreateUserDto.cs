using System.ComponentModel.DataAnnotations;

namespace Nawel.Api.DTOs;

public class CreateUserDto
{
    [Required]
    [MinLength(3)]
    public string Login { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    [Required]
    public int FamilyId { get; set; }

    public bool IsChildren { get; set; } = false;
}
