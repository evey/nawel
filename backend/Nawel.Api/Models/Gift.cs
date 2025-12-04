using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nawel.Api.Models;

[Table("gifts")]
public class Gift
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("list_id")]
    public int ListId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [MaxLength(500)]
    [Column("image")]
    public string? Image { get; set; }

    [MaxLength(500)]
    [Column("link")]
    public string? Link { get; set; }

    [Column("cost", TypeName = "decimal(10,2)")]
    public decimal? Cost { get; set; }

    [MaxLength(3)]
    [Column("currency")]
    public string? Currency { get; set; }

    [Column("available")]
    public bool Available { get; set; } = true;

    [Column("taken_by")]
    public int? TakenBy { get; set; }

    [Column("is_group_gift")]
    public bool IsGroupGift { get; set; } = false;

    [Column("comment", TypeName = "text")]
    public string? Comment { get; set; }

    [Required]
    [Column("year")]
    public int Year { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(ListId))]
    public virtual GiftList? List { get; set; }

    [ForeignKey(nameof(TakenBy))]
    public virtual User? TakenByUser { get; set; }

    public virtual ICollection<GiftParticipation> Participations { get; set; } = new List<GiftParticipation>();
}
