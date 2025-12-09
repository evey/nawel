using Nawel.Api.DTOs;
using Nawel.Api.Models;

namespace Nawel.Api.Extensions;

public static class GiftExtensions
{
    /// <summary>
    /// Convertit un Gift en GiftDto avec toutes les informations de réservation
    /// </summary>
    public static GiftDto ToDto(this Gift gift)
    {
        return new GiftDto
        {
            Id = gift.Id,
            Name = gift.Name,
            Description = gift.Description,
            Url = gift.Link,
            ImageUrl = gift.Image,
            Price = gift.Cost,
            Year = gift.Year,
            IsTaken = !gift.Available,
            TakenByUserId = gift.TakenBy,
            TakenByUserName = gift.TakenByUser != null
                ? gift.TakenByUser.FirstName ?? gift.TakenByUser.Login
                : null,
            Comment = gift.Comment,
            IsGroupGift = gift.IsGroupGift,
            ParticipantCount = gift.Participations?.Count ?? 0,
            ParticipantNames = gift.Participations?
                .Select(p => p.User != null ? (p.User.FirstName ?? p.User.Login) : "Unknown")
                .ToList()
        };
    }
}
