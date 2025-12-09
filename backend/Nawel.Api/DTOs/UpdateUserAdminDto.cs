using System.ComponentModel.DataAnnotations;

namespace Nawel.Api.DTOs;

public class UpdateUserAdminDto
{
    [EmailAddress]
    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public int? FamilyId { get; set; }

    public bool? IsChildren { get; set; }

    public bool? IsAdmin { get; set; }
}
