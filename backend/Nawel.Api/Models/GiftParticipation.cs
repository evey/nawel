using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nawel.Api.Models;

[Table("gift_participation")]
public class GiftParticipation
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("gift_id")]
    public int GiftId { get; set; }

    [Required]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(GiftId))]
    public virtual Gift? Gift { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}
