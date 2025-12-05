using System.ComponentModel.DataAnnotations;

namespace Nawel.Api.DTOs;

public class ExtractProductInfoRequest
{
    [Required]
    [Url]
    public string Url { get; set; } = string.Empty;
}
