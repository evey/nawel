using System.ComponentModel.DataAnnotations;

namespace Nawel.Api.DTOs;

public class UpdateFamilyDto
{
    [Required]
    [MinLength(2)]
    public string Name { get; set; } = string.Empty;
}
