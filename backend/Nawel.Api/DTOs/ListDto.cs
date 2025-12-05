namespace Nawel.Api.DTOs;

public class ListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Avatar { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
}

public class UserListDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int GiftCount { get; set; }
    public bool IsChildren { get; set; }
}

public class FamilyListsDto
{
    public int FamilyId { get; set; }
    public string FamilyName { get; set; } = string.Empty;
    public List<UserListDto> Lists { get; set; } = new();
}

public class ListsByFamilyDto
{
    public List<FamilyListsDto> Families { get; set; } = new();
}
