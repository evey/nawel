namespace Nawel.Api.DTOs;

public class GiftDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Url { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Price { get; set; }
    public int Year { get; set; }
    public bool IsTaken { get; set; }
    public int? TakenByUserId { get; set; }
    public string? TakenByUserName { get; set; }
    public string? Comment { get; set; }
    public bool IsGroupGift { get; set; }
    public int ParticipantCount { get; set; }
    public List<string>? ParticipantNames { get; set; }
}

public class CreateGiftDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Url { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Price { get; set; }
    public bool IsGroupGift { get; set; }
}

public class UpdateGiftDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Price { get; set; }
    public bool? IsGroupGift { get; set; }
}

public class ReserveGiftDto
{
    public string? Comment { get; set; }
}
