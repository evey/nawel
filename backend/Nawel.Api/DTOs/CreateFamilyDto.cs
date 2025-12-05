using System.ComponentModel.DataAnnotations;

namespace Nawel.Api.DTOs;

public class CreateFamilyDto
{
    [Required]
    [MinLength(2)]
    public string Name { get; set; } = string.Empty;
}
