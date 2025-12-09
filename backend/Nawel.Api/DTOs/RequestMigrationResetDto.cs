using System.ComponentModel.DataAnnotations;

namespace Nawel.Api.DTOs;

/// <summary>
/// DTO for requesting a password migration reset for legacy MD5 passwords
/// </summary>
public class RequestMigrationResetDto
{
    [Required(ErrorMessage = "Login is required")]
    public string Login { get; set; } = string.Empty;
}
