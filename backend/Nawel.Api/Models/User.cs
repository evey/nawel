using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nawel.Api.Models;

[Table("user")]
public class User
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("login")]
    public string Login { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("pwd")]
    public string Password { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("email")]
    public string? Email { get; set; }

    [MaxLength(255)]
    [Column("first_name")]
    public string? FirstName { get; set; }

    [MaxLength(255)]
    [Column("last_name")]
    public string? LastName { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("avatar")]
    public string Avatar { get; set; } = "avatar.png";

    [MaxLength(255)]
    [Column("pseudo")]
    public string? Pseudo { get; set; }

    [Column("notify_list_edit")]
    public bool NotifyListEdit { get; set; } = false;

    [Column("notify_gift_taken")]
    public bool NotifyGiftTaken { get; set; } = false;

    [Column("display_popup")]
    public bool DisplayPopup { get; set; } = true;

    [MaxLength(255)]
    [Column("reset_token")]
    public string? ResetToken { get; set; }

    [Column("token_expiry")]
    public DateTime? TokenExpiry { get; set; }

    [Column("isChildren")]
    public bool IsChildren { get; set; } = false;

    [Required]
    [Column("family_id")]
    public int FamilyId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(FamilyId))]
    public virtual Family? Family { get; set; }

    public virtual GiftList? List { get; set; }
    public virtual ICollection<Gift> TakenGifts { get; set; } = new List<Gift>();
    public virtual ICollection<GiftParticipation> GiftParticipations { get; set; } = new List<GiftParticipation>();
}
