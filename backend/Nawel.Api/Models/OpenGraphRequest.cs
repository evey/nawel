using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nawel.Api.Models;

[Table("opengraph_request")]
public class OpenGraphRequest
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(2048)]
    [Column("url")]
    public string Url { get; set; } = string.Empty;

    [Required]
    [Column("user_id")]
    public int UserId { get; set; }

    [Required]
    [Column("success")]
    public bool Success { get; set; }

    [MaxLength(500)]
    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}
