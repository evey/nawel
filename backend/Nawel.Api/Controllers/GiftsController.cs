using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nawel.Api.Data;
using Nawel.Api.DTOs;
using Nawel.Api.Models;
using Nawel.Api.Services.Email;
using System.Security.Claims;

namespace Nawel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GiftsController : ControllerBase
{
    private readonly NawelDbContext _context;
    private readonly ILogger<GiftsController> _logger;
    private readonly IEmailService _emailService;
    private readonly INotificationDebouncer _notificationDebouncer;
    private readonly IReservationNotificationDebouncer _reservationNotificationDebouncer;

    public GiftsController(NawelDbContext context, ILogger<GiftsController> logger, IEmailService emailService, INotificationDebouncer notificationDebouncer, IReservationNotificationDebouncer reservationNotificationDebouncer)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
        _notificationDebouncer = notificationDebouncer;
        _reservationNotificationDebouncer = reservationNotificationDebouncer;
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
                    Comment = g.Comment,
                    IsGroupGift = g.IsGroupGift,
                    ParticipantCount = g.Participations.Count,
                    ParticipantNames = g.Participations.Select(p => p.User.FirstName ?? p.User.Login).ToList()
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

    // GET: api/gifts/manage-child/{childId}
    [HttpGet("manage-child/{childId}")]
    public async Task<ActionResult<IEnumerable<GiftDto>>> GetChildGifts(int childId, [FromQuery] int? year = null)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentYear = year ?? DateTime.Now.Year;

            // Get current user
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (currentUser == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Verify current user is an adult
            if (currentUser.IsChildren)
            {
                return Forbid();
            }

            // Get child user
            var childUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == childId);

            if (childUser == null)
            {
                return NotFound(new { message = "Child not found" });
            }

            // Verify child is in same family
            if (childUser.FamilyId != currentUser.FamilyId)
            {
                return Forbid();
            }

            // Verify child is actually a child account
            if (!childUser.IsChildren)
            {
                return BadRequest(new { message = "User is not a child account" });
            }

            var childList = await _context.Lists
                .FirstOrDefaultAsync(l => l.UserId == childId);

            if (childList == null)
            {
                // Return empty list if no list exists yet
                return Ok(new List<GiftDto>());
            }

            var gifts = await _context.Gifts
                .Include(g => g.TakenByUser)
                .Include(g => g.Participations)
                    .ThenInclude(p => p.User)
                .Where(g => g.ListId == childList.Id && g.Year == currentYear)
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
                    Comment = g.Comment,
                    IsGroupGift = g.IsGroupGift,
                    ParticipantCount = g.Participations.Count,
                    ParticipantNames = g.Participations.Select(p => p.User.FirstName ?? p.User.Login).ToList()
                })
                .ToListAsync();

            return Ok(gifts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting child gifts");
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
                    Comment = g.Comment,
                    IsGroupGift = g.IsGroupGift,
                    ParticipantCount = g.Participations.Count,
                    ParticipantNames = g.Participations.Select(p => p.User.FirstName ?? p.User.Login).ToList()
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

    // POST: api/gifts/manage-child/{childId}
    [HttpPost("manage-child/{childId}")]
    public async Task<ActionResult<GiftDto>> CreateGiftForChild(int childId, [FromBody] CreateGiftDto giftDto)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentYear = DateTime.Now.Year;

            // Get current user
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (currentUser == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Verify current user is an adult
            if (currentUser.IsChildren)
            {
                return Forbid();
            }

            // Get child user
            var childUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == childId);

            if (childUser == null)
            {
                return NotFound(new { message = "Child not found" });
            }

            // Verify child is in same family
            if (childUser.FamilyId != currentUser.FamilyId)
            {
                return Forbid();
            }

            // Verify child is actually a child account
            if (!childUser.IsChildren)
            {
                return BadRequest(new { message = "User is not a child account" });
            }

            var childList = await _context.Lists
                .FirstOrDefaultAsync(l => l.UserId == childId);

            if (childList == null)
            {
                // Create a new list if it doesn't exist
                childList = new GiftList
                {
                    Name = $"Liste {currentYear}",
                    UserId = childId
                };
                _context.Lists.Add(childList);
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
                ListId = childList.Id,
                IsGroupGift = giftDto.IsGroupGift,
                Available = true
            };

            _context.Gifts.Add(gift);
            await _context.SaveChangesAsync();

            // Schedule aggregated email notification for list edit (using child's name)
            _notificationDebouncer.ScheduleListEditNotification(
                childId,
                childUser.FirstName ?? childUser.Login,
                "add",
                gift.Name);

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
                Comment = gift.Comment,
                IsGroupGift = gift.IsGroupGift,
                ParticipantCount = 0
            };

            return CreatedAtAction(nameof(GetChildGifts), new { childId = childId, year = currentYear }, resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating gift for child");
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

            // Schedule aggregated email notification for list edit
            var currentUser = await _context.Users.FindAsync(currentUserId);
            if (currentUser != null)
            {
                _notificationDebouncer.ScheduleListEditNotification(
                    currentUserId,
                    currentUser.FirstName ?? currentUser.Login,
                    "add",
                    gift.Name);
            }

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
                Comment = gift.Comment,
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
                    .ThenInclude(l => l!.User)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gift == null)
            {
                return NotFound(new { message = "Gift not found" });
            }

            // Get current user
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (currentUser == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var giftOwnerId = gift.List?.UserId ?? 0;

            // Check if user owns this gift OR if user is managing a child's list
            bool canModify = giftOwnerId == currentUserId;

            if (!canModify && !currentUser.IsChildren)
            {
                // Check if this is a child's gift that the current user can manage
                var listOwner = gift.List?.User;
                if (listOwner != null && listOwner.IsChildren && listOwner.FamilyId == currentUser.FamilyId)
                {
                    canModify = true;
                }
            }

            if (!canModify)
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

            // Schedule aggregated email notification for list edit (use gift owner's ID)
            var giftOwner = gift.List?.User ?? await _context.Users.FindAsync(giftOwnerId);
            if (giftOwner != null)
            {
                _notificationDebouncer.ScheduleListEditNotification(
                    giftOwnerId,
                    giftOwner.FirstName ?? giftOwner.Login,
                    "update",
                    gift.Name);
            }

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
                Comment = gift.Comment,
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
                    .ThenInclude(l => l!.User)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gift == null)
            {
                return NotFound(new { message = "Gift not found" });
            }

            // Get current user
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (currentUser == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var giftOwnerId = gift.List?.UserId ?? 0;

            // Check if user owns this gift OR if user is managing a child's list
            bool canModify = giftOwnerId == currentUserId;

            if (!canModify && !currentUser.IsChildren)
            {
                // Check if this is a child's gift that the current user can manage
                var listOwner = gift.List?.User;
                if (listOwner != null && listOwner.IsChildren && listOwner.FamilyId == currentUser.FamilyId)
                {
                    canModify = true;
                }
            }

            if (!canModify)
            {
                return Forbid();
            }

            var giftName = gift.Name;
            _context.Gifts.Remove(gift);
            await _context.SaveChangesAsync();

            // Schedule aggregated email notification for list edit (use gift owner's ID)
            var giftOwnerUser = gift.List?.User ?? await _context.Users.FindAsync(giftOwnerId);
            if (giftOwnerUser != null)
            {
                _notificationDebouncer.ScheduleListEditNotification(
                    giftOwnerId,
                    giftOwnerUser.FirstName ?? giftOwnerUser.Login,
                    "delete",
                    giftName);
            }

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
    public async Task<ActionResult> ReserveGift(int id, [FromBody] ReserveGiftDto? reserveDto = null)
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

            // Check if user is already participating
            var existingParticipation = await _context.GiftParticipations
                .FirstOrDefaultAsync(p => p.GiftId == id && p.UserId == currentUserId);

            if (existingParticipation != null)
            {
                return BadRequest(new { message = "Already participating in this gift" });
            }

            // If gift is already taken by someone else (not group gift), convert to group gift
            if (!gift.Available && !gift.IsGroupGift && gift.TakenBy.HasValue && gift.TakenBy.Value != currentUserId)
            {
                // Convert to group gift: add original reserver as first participant
                gift.IsGroupGift = true;

                var originalParticipation = new GiftParticipation
                {
                    GiftId = id,
                    UserId = gift.TakenBy.Value
                };
                _context.GiftParticipations.Add(originalParticipation);

                // Add new participant
                var newParticipation = new GiftParticipation
                {
                    GiftId = id,
                    UserId = currentUserId
                };
                _context.GiftParticipations.Add(newParticipation);
            }
            else if (gift.IsGroupGift || (!gift.Available && gift.IsGroupGift))
            {
                // Already a group gift, just add participation
                var participation = new GiftParticipation
                {
                    GiftId = id,
                    UserId = currentUserId
                };
                _context.GiftParticipations.Add(participation);
            }
            else if (gift.Available)
            {
                // First reservation: single gift reservation
                gift.Available = false;
                gift.TakenBy = currentUserId;
            }
            else
            {
                // Gift already reserved by current user (shouldn't happen but just in case)
                return BadRequest(new { message = "Gift already reserved" });
            }

            // Add comment if provided
            if (!string.IsNullOrWhiteSpace(reserveDto?.Comment))
            {
                gift.Comment = reserveDto.Comment;
            }

            await _context.SaveChangesAsync();

            // Schedule aggregated email notification for reservation
            var giftOwner = await _context.Users.FindAsync(gift.List!.UserId);
            var currentUser = await _context.Users.FindAsync(currentUserId);

            if (giftOwner != null && currentUser != null)
            {
                var ownerName = giftOwner.FirstName ?? giftOwner.Login;
                var reserverName = currentUser.FirstName ?? currentUser.Login;
                var actionType = gift.IsGroupGift ? "participate" : "reserve";

                _reservationNotificationDebouncer.ScheduleReservationNotification(
                    gift.List!.UserId,
                    ownerName,
                    reserverName,
                    actionType,
                    gift.Name,
                    gift.Comment);
            }

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

            var wasGroupGift = gift.IsGroupGift;
            var giftName = gift.Name;

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

                // Check remaining participants after removal
                var remainingParticipants = await _context.GiftParticipations
                    .Where(p => p.GiftId == id && p.UserId != currentUserId)
                    .ToListAsync();

                if (remainingParticipants.Count == 0)
                {
                    // No participants left, mark as available
                    gift.IsGroupGift = false;
                    gift.Available = true;
                    gift.TakenBy = null;
                }
                else if (remainingParticipants.Count == 1)
                {
                    // Only one participant left, convert back to single gift
                    gift.IsGroupGift = false;
                    gift.TakenBy = remainingParticipants[0].UserId;

                    // Remove the last participation entry
                    _context.GiftParticipations.Remove(remainingParticipants[0]);
                }
                // else: still multiple participants, keep as group gift
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

            // Schedule aggregated email notification for unreservation
            var giftOwner = await _context.Users.FindAsync(gift.List!.UserId);
            var currentUser = await _context.Users.FindAsync(currentUserId);

            if (giftOwner != null && currentUser != null)
            {
                var ownerName = giftOwner.FirstName ?? giftOwner.Login;
                var unreserverName = currentUser.FirstName ?? currentUser.Login;
                var actionType = wasGroupGift ? "unparticipate" : "unreserve";

                _reservationNotificationDebouncer.ScheduleReservationNotification(
                    gift.List!.UserId,
                    ownerName,
                    unreserverName,
                    actionType,
                    giftName);
            }

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

    // POST: api/gifts/add-marie-group-gift
    [HttpPost("add-marie-group-gift")]
    public async Task<ActionResult> AddMarieGroupGift()
    {
        try
        {
            await AddTestData.AddMarieGroupGift(_context);
            return Ok(new { message = "Group gift created successfully for Marie" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group gift for Marie");
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
