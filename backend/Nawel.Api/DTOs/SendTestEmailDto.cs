using System.ComponentModel.DataAnnotations;

namespace Nawel.Api.DTOs;

public class SendTestEmailDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Type d'email à tester : "list_edited", "gift_reserved", "migration_reset"
    /// </summary>
    [Required]
    public string Type { get; set; } = string.Empty;
}
