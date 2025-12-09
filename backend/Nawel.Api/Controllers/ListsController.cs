using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nawel.Api.Data;
using Nawel.Api.DTOs;
using System.Security.Claims;

namespace Nawel.Api.Controllers;

/// <summary>
/// Contrôleur pour la gestion des listes de cadeaux.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ListsController : ControllerBase
{
    private readonly NawelDbContext _context;
    private readonly ILogger<ListsController> _logger;

    public ListsController(NawelDbContext context, ILogger<ListsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Récupère toutes les listes de cadeaux organisées par famille (sauf celle de l'utilisateur connecté).
    /// </summary>
    /// <returns>Les listes de cadeaux groupées par famille avec le nombre de cadeaux pour l'année en cours.</returns>
    /// <response code="200">Listes récupérées avec succès.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="500">Erreur serveur lors de la récupération des listes.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ListsByFamilyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ListsByFamilyDto>> GetAllLists()
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentYear = DateTime.UtcNow.Year;

            // Get all families
            var families = await _context.Families.ToListAsync();

            var result = new List<FamilyListsDto>();

            foreach (var family in families)
            {
                // Get users for this family
                var users = await _context.Users
                    .Include(u => u.List)
                        .ThenInclude(l => l!.Gifts)
                    .Where(u => u.FamilyId == family.Id
                            && u.Id != currentUserId
                            && u.Id != 1) // Exclude current user and admin
                    .ToListAsync();

                if (users.Any())
                {
                    var userLists = users
                        .Select(u => new UserListDto
                        {
                            UserId = u.Id,
                            UserName = u.FirstName ?? u.Login,
                            AvatarUrl = u.Avatar,
                            GiftCount = u.List?.Gifts.Count(g => g.Year == currentYear) ?? 0,
                            IsChildren = u.IsChildren
                        })
                        .ToList();

                    result.Add(new FamilyListsDto
                    {
                        FamilyId = family.Id,
                        FamilyName = family.Name,
                        Lists = userLists
                    });
                }
            }

            return Ok(new ListsByFamilyDto { Families = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lists");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Récupère la liste de cadeaux de l'utilisateur connecté.
    /// </summary>
    /// <returns>Les informations de la liste de l'utilisateur (ID, nom, avatar, famille).</returns>
    /// <response code="200">Liste récupérée avec succès.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="404">Liste non trouvée.</response>
    /// <response code="500">Erreur serveur lors de la récupération de la liste.</response>
    [HttpGet("mine")]
    [ProducesResponseType(typeof(ListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ListDto>> GetMyList()
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var list = await _context.Lists
                .Include(l => l.User)
                    .ThenInclude(u => u!.Family)
                .FirstOrDefaultAsync(l => l.UserId == currentUserId);

            if (list == null)
            {
                return NotFound(new { message = "List not found" });
            }

            var listDto = new ListDto
            {
                Id = list.Id,
                Name = list.Name,
                UserId = list.UserId,
                Avatar = list.User!.Avatar,
                FamilyName = list.User.Family!.Name
            };

            return Ok(listDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user list");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}
