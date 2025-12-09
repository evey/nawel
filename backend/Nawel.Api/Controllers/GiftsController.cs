using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nawel.Api.Data;
using Nawel.Api.DTOs;
using Nawel.Api.Extensions;
using Nawel.Api.Models;
using Nawel.Api.Services.Email;
using System.Security.Claims;

namespace Nawel.Api.Controllers;

/// <summary>
/// Contrôleur pour la gestion des cadeaux (CRUD, réservations, imports, cadeaux groupés).
/// </summary>
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

    /// <summary>
    /// Récupère la liste des années pour lesquelles l'utilisateur a des cadeaux dans sa liste.
    /// </summary>
    /// <returns>La liste des années disponibles (incluant l'année en cours).</returns>
    /// <response code="200">Liste des années récupérée avec succès.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="500">Erreur serveur lors de la récupération.</response>
    [HttpGet("years")]
    [ProducesResponseType(typeof(IEnumerable<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Importe les cadeaux non réservés d'une année passée vers l'année en cours.
    /// </summary>
    /// <param name="year">L'année source depuis laquelle importer les cadeaux.</param>
    /// <returns>Le nombre de cadeaux importés.</returns>
    /// <response code="200">Cadeaux importés avec succès (ou aucun cadeau à importer).</response>
    /// <response code="400">Tentative d'import depuis l'année en cours ou future.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="404">Liste de l'utilisateur non trouvée.</response>
    /// <response code="500">Erreur serveur lors de l'import.</response>
    [HttpPost("import-from-year/{year}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Récupère la liste des cadeaux de l'utilisateur connecté pour une année donnée.
    /// </summary>
    /// <param name="year">L'année pour laquelle récupérer les cadeaux (année en cours par défaut).</param>
    /// <returns>La liste des cadeaux avec leurs statuts (réservé, cadeaux groupés, etc.).</returns>
    /// <response code="200">Liste des cadeaux récupérée avec succès.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="500">Erreur serveur lors de la récupération.</response>
    [HttpGet("my-list")]
    [ProducesResponseType(typeof(IEnumerable<GiftDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                .ToListAsync();

            var giftDtos = gifts.Select(g => g.ToDto()).ToList();

            return Ok(giftDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user gifts");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Récupère la liste des cadeaux d'un enfant (pour gestion parent).
    /// </summary>
    /// <param name="childId">L'ID de l'enfant.</param>
    /// <param name="year">L'année pour laquelle récupérer les cadeaux (année en cours par défaut).</param>
    /// <returns>La liste des cadeaux de l'enfant.</returns>
    /// <response code="200">Liste des cadeaux récupérée avec succès.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="403">L'utilisateur n'est pas le parent de cet enfant.</response>
    /// <response code="404">Enfant ou liste non trouvée.</response>
    /// <response code="500">Erreur serveur lors de la récupération.</response>
    [HttpGet("manage-child/{childId}")]
    [ProducesResponseType(typeof(IEnumerable<GiftDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                .ToListAsync();

            var giftDtos = gifts.Select(g => g.ToDto()).ToList();

            return Ok(giftDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting child gifts");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Récupère la liste des cadeaux d'un autre utilisateur pour consultation/réservation.
    /// </summary>
    /// <param name="userId">L'ID de l'utilisateur dont on veut consulter la liste.</param>
    /// <param name="year">L'année pour laquelle récupérer les cadeaux (année en cours par défaut).</param>
    /// <returns>La liste des cadeaux de l'utilisateur avec statuts de réservation.</returns>
    /// <response code="200">Liste des cadeaux récupérée avec succès.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="404">Utilisateur ou liste non trouvée.</response>
    /// <response code="500">Erreur serveur lors de la récupération.</response>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(IEnumerable<GiftDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                .ToListAsync();

            var giftDtos = gifts.Select(g => g.ToDto()).ToList();

            return Ok(giftDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user gifts");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Crée un nouveau cadeau pour un enfant (gestion parent).
    /// </summary>
    /// <param name="childId">L'ID de l'enfant.</param>
    /// <param name="giftDto">Les informations du nouveau cadeau.</param>
    /// <returns>Le cadeau créé avec son ID.</returns>
    /// <response code="200">Cadeau créé avec succès.</response>
    /// <response code="400">Données invalides ou utilisateur cible n'est pas un enfant.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="403">L'utilisateur n'est pas adulte ou pas dans la même famille que l'enfant.</response>
    /// <response code="404">Utilisateur ou enfant non trouvé, ou liste non trouvée.</response>
    /// <response code="500">Erreur serveur lors de la création.</response>
    [HttpPost("manage-child/{childId}")]
    [ProducesResponseType(typeof(GiftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Crée un nouveau cadeau dans la liste de l'utilisateur connecté.
    /// </summary>
    /// <param name="giftDto">Les informations du nouveau cadeau (nom, description, lien, prix, image, isGroupGift, etc.).</param>
    /// <returns>Le cadeau créé avec son ID.</returns>
    /// <response code="201">Cadeau créé avec succès.</response>
    /// <response code="400">Données invalides.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="500">Erreur serveur lors de la création.</response>
    /// <remarks>
    /// Si l'utilisateur n'a pas encore de liste, une liste est automatiquement créée.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(GiftDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Met à jour un cadeau existant dans la liste de l'utilisateur connecté.
    /// </summary>
    /// <param name="id">L'ID du cadeau à mettre à jour.</param>
    /// <param name="updateDto">Les nouvelles informations du cadeau.</param>
    /// <returns>Le cadeau mis à jour.</returns>
    /// <response code="200">Cadeau mis à jour avec succès.</response>
    /// <response code="400">Données invalides.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="403">Le cadeau n'appartient pas à l'utilisateur.</response>
    /// <response code="404">Cadeau non trouvé.</response>
    /// <response code="500">Erreur serveur lors de la mise à jour.</response>
    /// <remarks>
    /// Les notifications d'édition de liste sont envoyées aux autres utilisateurs ayant réservé ce cadeau (avec debouncing de 2 minutes).
    /// </remarks>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(GiftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Supprime un cadeau de la liste de l'utilisateur connecté.
    /// </summary>
    /// <param name="id">L'ID du cadeau à supprimer.</param>
    /// <returns>Un message de confirmation.</returns>
    /// <response code="200">Cadeau supprimé avec succès.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="403">Le cadeau n'appartient pas à l'utilisateur.</response>
    /// <response code="404">Cadeau non trouvé.</response>
    /// <response code="500">Erreur serveur lors de la suppression.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Réserve un cadeau (ou participe à un cadeau groupé).
    /// </summary>
    /// <param name="id">L'ID du cadeau à réserver.</param>
    /// <param name="reserveDto">Commentaire optionnel pour la réservation.</param>
    /// <returns>Un message de confirmation.</returns>
    /// <response code="200">Cadeau réservé avec succès (ou participation ajoutée pour cadeau groupé).</response>
    /// <response code="400">Cadeau déjà entièrement réservé, ou tentative de réserver son propre cadeau.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="404">Cadeau non trouvé.</response>
    /// <response code="500">Erreur serveur lors de la réservation.</response>
    /// <remarks>
    /// Pour les cadeaux groupés, l'utilisateur est ajouté à la liste des participants.
    /// Pour les cadeaux classiques, le cadeau est marqué comme pris par cet utilisateur.
    /// Les notifications de réservation sont envoyées au propriétaire du cadeau (avec debouncing de 2 minutes).
    /// </remarks>
    [HttpPost("{id}/reserve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

            // Validate permissions
            var validationError = await ValidateReservationPermissions(gift, currentUserId, id);
            if (validationError != null)
            {
                return validationError;
            }

            // Process reservation
            ProcessReservation(gift, id, currentUserId);

            // Add comment if provided
            if (!string.IsNullOrWhiteSpace(reserveDto?.Comment))
            {
                gift.Comment = reserveDto.Comment;
            }

            await _context.SaveChangesAsync();

            // Send notification
            await SendReservationNotification(gift, currentUserId);

            return Ok(new { message = "Gift reserved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving gift");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Annule la réservation d'un cadeau (ou retire la participation à un cadeau groupé).
    /// </summary>
    /// <param name="id">L'ID du cadeau dont on veut annuler la réservation.</param>
    /// <returns>Un message de confirmation.</returns>
    /// <response code="200">Réservation annulée avec succès (ou participation retirée).</response>
    /// <response code="400">L'utilisateur n'a pas réservé ce cadeau.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="404">Cadeau non trouvé.</response>
    /// <response code="500">Erreur serveur lors de l'annulation de la réservation.</response>
    /// <remarks>
    /// Pour les cadeaux groupés, retire l'utilisateur de la liste des participants.
    /// Pour les cadeaux classiques, libère le cadeau (Available = true, TakenBy = null).
    /// Les notifications d'annulation sont envoyées au propriétaire du cadeau (avec debouncing de 2 minutes).
    /// </remarks>
    [HttpDelete("{id}/reserve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    // Private helper methods for ReserveGift
    private async Task<ActionResult?> ValidateReservationPermissions(Gift gift, int currentUserId, int giftId)
    {
        // Can't reserve own gift
        if (gift.List?.UserId == currentUserId)
        {
            return BadRequest(new { message = "Cannot reserve your own gift" });
        }

        // Check if user is already participating
        var existingParticipation = await _context.GiftParticipations
            .FirstOrDefaultAsync(p => p.GiftId == giftId && p.UserId == currentUserId);

        if (existingParticipation != null)
        {
            return BadRequest(new { message = "Already participating in this gift" });
        }

        return null; // No validation errors
    }

    private void ProcessReservation(Gift gift, int giftId, int currentUserId)
    {
        // If gift is already taken by someone else (not group gift), convert to group gift
        if (!gift.Available && !gift.IsGroupGift && gift.TakenBy.HasValue && gift.TakenBy.Value != currentUserId)
        {
            // Convert to group gift: add original reserver as first participant
            gift.IsGroupGift = true;

            var originalParticipation = new GiftParticipation
            {
                GiftId = giftId,
                UserId = gift.TakenBy.Value
            };
            _context.GiftParticipations.Add(originalParticipation);

            // Add new participant
            var newParticipation = new GiftParticipation
            {
                GiftId = giftId,
                UserId = currentUserId
            };
            _context.GiftParticipations.Add(newParticipation);
        }
        else if (gift.IsGroupGift || (!gift.Available && gift.IsGroupGift))
        {
            // Already a group gift, just add participation
            var participation = new GiftParticipation
            {
                GiftId = giftId,
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
    }

    private async Task SendReservationNotification(Gift gift, int currentUserId)
    {
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
    }
}
