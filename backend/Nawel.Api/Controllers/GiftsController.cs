using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nawel.Api.Data;
using Nawel.Api.DTOs;
using Nawel.Api.Models;
using System.Security.Claims;

namespace Nawel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GiftsController : ControllerBase
{
    private readonly NawelDbContext _context;
    private readonly ILogger<GiftsController> _logger;

    public GiftsController(NawelDbContext context, ILogger<GiftsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/gifts/years
    [HttpGet("years")]
    public async Task<ActionResult<IEnumerable<int>>> GetAvailableYears()
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var userList = await _context.Lists
                .FirstOrDefaultAsync(l => l.UserId == currentUserId);

            if (userList == null)
            {
                return Ok(new List<int> { DateTime.Now.Year });
            }

            var years = await _context.Gifts
                .Where(g => g.ListId == userList.Id)
                .Select(g => g.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            // Always include current year
            if (!years.Contains(DateTime.Now.Year))
            {
                years.Insert(0, DateTime.Now.Year);
            }

            return Ok(years);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available years");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    // POST: api/gifts/import-from-year/{year}
    [HttpPost("import-from-year/{year}")]
    public async Task<ActionResult> ImportUnpurchasedGifts(int year)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentYear = DateTime.Now.Year;

            if (year >= currentYear)
            {
                return BadRequest(new { message = "Can only import from past years" });
            }

            var userList = await _context.Lists
                .FirstOrDefaultAsync(l => l.UserId == currentUserId);

            if (userList == null)
            {
                return NotFound(new { message = "List not found" });
            }

            // Get unpurchased gifts from specified year
            var unpurchasedGifts = await _context.Gifts
                .Where(g => g.ListId == userList.Id && g.Year == year && g.Available)
                .ToListAsync();

            if (!unpurchasedGifts.Any())
            {
                return Ok(new { message = "No unpurchased gifts to import", count = 0 });
            }

            // Create new gifts for current year
            var newGifts = unpurchasedGifts.Select(g => new Gift
            {
                Name = g.Name,
                Description = g.Description,
                Link = g.Link,
                Cost = g.Cost,
                Currency = g.Currency,
                Image = g.Image,
                Year = currentYear,
                ListId = userList.Id,
                IsGroupGift = g.IsGroupGift,
                Available = true
            }).ToList();

            _context.Gifts.AddRange(newGifts);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{newGifts.Count} gifts imported successfully", count = newGifts.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing gifts");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    // GET: api/gifts/my-list
    [HttpGet("my-list")]
    public async Task<ActionResult<IEnumerable<GiftDto>>> GetMyGifts([FromQuery] int? year = null)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentYear = year ?? DateTime.Now.Year;

            var userList = await _context.Lists
                .FirstOrDefaultAsync(l => l.UserId == currentUserId);

            if (userList == null)
            {
                // Return empty list if no list exists yet
                return Ok(new List<GiftDto>());
            }

            var gifts = await _context.Gifts
                .Include(g => g.TakenByUser)
                .Include(g => g.Participations)
                    .ThenInclude(p => p.User)
                .Where(g => g.ListId == userList.Id && g.Year == currentYear)
                .OrderBy(g => g.Name)
                .Select(g => new GiftDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    Url = g.Link,
                    ImageUrl = g.Image,
                    Price = g.Cost,
                    Year = g.Year,
                    IsTaken = !g.Available,
                    TakenByUserId = g.TakenBy,
                    TakenByUserName = g.TakenByUser != null ? g.TakenByUser.FirstName ?? g.TakenByUser.Login : null,
                    IsGroupGift = g.IsGroupGift,
                    ParticipantCount = g.Participations.Count
                })
                .ToListAsync();

            return Ok(gifts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user gifts");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    // GET: api/gifts/{userId}
    [HttpGet("{userId}")]
    public async Task<ActionResult<IEnumerable<GiftDto>>> GetUserGifts(int userId, [FromQuery] int? year = null)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentYear = year ?? DateTime.Now.Year;

            // Don't allow viewing own list through this endpoint
            if (userId == currentUserId)
            {
                return BadRequest(new { message = "Use /my-list endpoint for your own gifts" });
            }

            var userList = await _context.Lists
                .FirstOrDefaultAsync(l => l.UserId == userId);

            if (userList == null)
            {
                return NotFound(new { message = "List not found" });
            }

            var gifts = await _context.Gifts
                .Include(g => g.TakenByUser)
                .Include(g => g.Participations)
                    .ThenInclude(p => p.User)
                .Where(g => g.ListId == userList.Id && g.Year == currentYear)
                .OrderBy(g => g.Name)
                .Select(g => new GiftDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    Url = g.Link,
                    ImageUrl = g.Image,
                    Price = g.Cost,
                    Year = g.Year,
                    IsTaken = !g.Available,
                    TakenByUserId = g.TakenBy,
                    TakenByUserName = g.TakenByUser != null ? g.TakenByUser.FirstName ?? g.TakenByUser.Login : null,
                    IsGroupGift = g.IsGroupGift,
                    ParticipantCount = g.Participations.Count
                })
                .ToListAsync();

            return Ok(gifts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user gifts");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    // POST: api/gifts
    [HttpPost]
    public async Task<ActionResult<GiftDto>> CreateGift([FromBody] CreateGiftDto giftDto)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentYear = DateTime.Now.Year;

            var userList = await _context.Lists
                .FirstOrDefaultAsync(l => l.UserId == currentUserId);

            if (userList == null)
            {
                // Create a new list if it doesn't exist
                userList = new GiftList
                {
                    Name = $"Liste {currentYear}",
                    UserId = currentUserId
                };
                _context.Lists.Add(userList);
                await _context.SaveChangesAsync();
            }

            var gift = new Gift
            {
                Name = giftDto.Name,
                Description = giftDto.Description,
                Link = giftDto.Url,
                Image = giftDto.ImageUrl,
                Cost = giftDto.Price,
                Year = currentYear,
                ListId = userList.Id,
                IsGroupGift = giftDto.IsGroupGift,
                Available = true
            };

            _context.Gifts.Add(gift);
            await _context.SaveChangesAsync();

            var resultDto = new GiftDto
            {
                Id = gift.Id,
                Name = gift.Name,
                Description = gift.Description,
                Url = gift.Link,
                ImageUrl = gift.Image,
                Price = gift.Cost,
                Year = gift.Year,
                IsTaken = false,
                IsGroupGift = gift.IsGroupGift,
                ParticipantCount = 0
            };

            return CreatedAtAction(nameof(GetMyGifts), new { year = currentYear }, resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating gift");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    // PUT: api/gifts/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<GiftDto>> UpdateGift(int id, [FromBody] UpdateGiftDto updateDto)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var gift = await _context.Gifts
                .Include(g => g.List)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gift == null)
            {
                return NotFound(new { message = "Gift not found" });
            }

            // Check if user owns this gift
            if (gift.List?.UserId != currentUserId)
            {
                return Forbid();
            }

            // Update only provided fields
            if (updateDto.Name != null) gift.Name = updateDto.Name;
            if (updateDto.Description != null) gift.Description = updateDto.Description;
            if (updateDto.Url != null) gift.Link = updateDto.Url;
            if (updateDto.ImageUrl != null) gift.Image = updateDto.ImageUrl;
            if (updateDto.Price.HasValue) gift.Cost = updateDto.Price;
            if (updateDto.IsGroupGift.HasValue) gift.IsGroupGift = updateDto.IsGroupGift.Value;

            await _context.SaveChangesAsync();

            var resultDto = new GiftDto
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
                IsGroupGift = gift.IsGroupGift,
                ParticipantCount = await _context.GiftParticipations.CountAsync(p => p.GiftId == gift.Id)
            };

            return Ok(resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating gift");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    // DELETE: api/gifts/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteGift(int id)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var gift = await _context.Gifts
                .Include(g => g.List)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gift == null)
            {
                return NotFound(new { message = "Gift not found" });
            }

            // Check if user owns this gift
            if (gift.List?.UserId != currentUserId)
            {
                return Forbid();
            }

            _context.Gifts.Remove(gift);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting gift");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    // POST: api/gifts/{id}/reserve
    [HttpPost("{id}/reserve")]
    public async Task<ActionResult> ReserveGift(int id)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var gift = await _context.Gifts
                .Include(g => g.List)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gift == null)
            {
                return NotFound(new { message = "Gift not found" });
            }

            // Can't reserve own gift
            if (gift.List?.UserId == currentUserId)
            {
                return BadRequest(new { message = "Cannot reserve your own gift" });
            }

            // Check if already taken
            if (!gift.Available && !gift.IsGroupGift)
            {
                return BadRequest(new { message = "Gift already reserved" });
            }

            if (gift.IsGroupGift)
            {
                // Add participation
                var existingParticipation = await _context.GiftParticipations
                    .FirstOrDefaultAsync(p => p.GiftId == id && p.UserId == currentUserId);

                if (existingParticipation != null)
                {
                    return BadRequest(new { message = "Already participating in this gift" });
                }

                var participation = new GiftParticipation
                {
                    GiftId = id,
                    UserId = currentUserId
                };

                _context.GiftParticipations.Add(participation);
            }
            else
            {
                // Single gift reservation
                gift.Available = false;
                gift.TakenBy = currentUserId;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Gift reserved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving gift");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    // DELETE: api/gifts/{id}/reserve
    [HttpDelete("{id}/reserve")]
    public async Task<ActionResult> UnreserveGift(int id)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var gift = await _context.Gifts
                .Include(g => g.List)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gift == null)
            {
                return NotFound(new { message = "Gift not found" });
            }

            if (gift.IsGroupGift)
            {
                // Remove participation
                var participation = await _context.GiftParticipations
                    .FirstOrDefaultAsync(p => p.GiftId == id && p.UserId == currentUserId);

                if (participation == null)
                {
                    return BadRequest(new { message = "Not participating in this gift" });
                }

                _context.GiftParticipations.Remove(participation);
            }
            else
            {
                // Check if user reserved this gift
                if (gift.TakenBy != currentUserId)
                {
                    return BadRequest(new { message = "You did not reserve this gift" });
                }

                gift.Available = true;
                gift.TakenBy = null;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Reservation removed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unreserving gift");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    // POST: api/gifts/add-test-data-2024
    [HttpPost("add-test-data-2024")]
    public async Task<ActionResult> AddTestData2024()
    {
        try
        {
            await AddTestData.AddSylvainTestGifts(_context);
            return Ok(new { message = "Test data added successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding test data");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST: api/gifts/add-claire-test-data-2024
    [HttpPost("add-claire-test-data-2024")]
    public async Task<ActionResult> AddClaireTestData2024()
    {
        try
        {
            await AddTestData.AddClaireTestGifts(_context);
            return Ok(new { message = "Test data added successfully for Claire" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding test data for Claire");
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
