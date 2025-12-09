using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nawel.Api.Data;
using Nawel.Api.DTOs;
using Nawel.Api.Models;
using System.Security.Claims;

namespace Nawel.Api.Controllers;

/// <summary>
/// Contrôleur pour les fonctionnalités d'administration (utilisateurs, familles, statistiques).
/// Tous les endpoints nécessitent le rôle administrateur.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")] // All admin endpoints require admin role
public class AdminController : ControllerBase
{
    private readonly NawelDbContext _context;
    private readonly ILogger<AdminController> _logger;
    private readonly IConfiguration _configuration;

    public AdminController(NawelDbContext context, ILogger<AdminController> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Récupère les statistiques globales de l'application (utilisateurs, familles, cadeaux, API OpenGraph).
    /// </summary>
    /// <returns>Les statistiques de l'application pour l'année en cours et les 12 derniers mois.</returns>
    /// <response code="200">Statistiques récupérées avec succès.</response>
    /// <response code="401">Non autorisé (réservé aux administrateurs).</response>
    /// <response code="500">Erreur serveur lors de la récupération des statistiques.</response>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetStats()
    {
        try
        {
            var currentYear = DateTime.UtcNow.Year;

            var totalUsers = await _context.Users.CountAsync(u => u.Id != 1);
            var totalFamilies = await _context.Families.CountAsync();
            var totalGifts = await _context.Gifts.CountAsync(g => g.Year == currentYear);
            var totalReservedGifts = await _context.Gifts.CountAsync(g => g.Year == currentYear && g.TakenBy != null);

            // OpenGraph requests this month
            var firstDayOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var openGraphRequestsThisMonth = await _context.OpenGraphRequests
                .Where(r => r.CreatedAt >= firstDayOfMonth)
                .CountAsync();

            // OpenGraph requests by month (last 12 months)
            var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-12);
            var requestsByMonth = await _context.OpenGraphRequests
                .Where(r => r.CreatedAt >= twelveMonthsAgo)
                .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
                .Select(g => new
                {
                    year = g.Key.Year,
                    month = g.Key.Month,
                    count = g.Count(),
                    successCount = g.Count(r => r.Success)
                })
                .OrderBy(x => x.year).ThenBy(x => x.month)
                .ToListAsync();

            return Ok(new
            {
                totalUsers,
                totalFamilies,
                totalGifts,
                totalReservedGifts,
                openGraphRequestsThisMonth,
                requestsByMonth
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin stats");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Récupère la liste de tous les utilisateurs (sauf l'admin système).
    /// </summary>
    /// <returns>La liste complète des utilisateurs avec leurs familles.</returns>
    /// <response code="200">Liste des utilisateurs récupérée avec succès.</response>
    /// <response code="401">Non autorisé (réservé aux administrateurs).</response>
    /// <response code="500">Erreur serveur lors de la récupération des utilisateurs.</response>
    [HttpGet("users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetAllUsers()
    {
        try
        {
            var users = await _context.Users
                .Include(u => u.Family)
                .Where(u => u.Id != 1) // Exclude admin
                .OrderBy(u => u.Family!.Name)
                .ThenBy(u => u.FirstName)
                .Select(u => new
                {
                    u.Id,
                    u.Login,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.Avatar,
                    u.Pseudo,
                    u.IsChildren,
                    u.IsAdmin,
                    u.FamilyId,
                    FamilyName = u.Family!.Name,
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Crée un nouvel utilisateur et sa liste de cadeaux associée.
    /// </summary>
    /// <param name="dto">Les informations du nouvel utilisateur (login, password, email, firstName, lastName, familyId, isChildren, isAdmin).</param>
    /// <returns>Un message de confirmation avec l'ID du nouvel utilisateur.</returns>
    /// <response code="200">Utilisateur créé avec succès.</response>
    /// <response code="400">Login ou email déjà existant.</response>
    /// <response code="401">Non autorisé (réservé aux administrateurs).</response>
    /// <response code="500">Erreur serveur lors de la création de l'utilisateur.</response>
    [HttpPost("users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        try
        {
            // Check if login already exists
            if (await _context.Users.AnyAsync(u => u.Login == dto.Login))
            {
                return BadRequest(new { message = "Ce nom d'utilisateur existe déjà" });
            }

            // Check if email already exists
            if (!string.IsNullOrEmpty(dto.Email) && await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest(new { message = "Cet email existe déjà" });
            }

            var user = new User
            {
                Login = dto.Login,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                FamilyId = dto.FamilyId,
                IsChildren = dto.IsChildren,
                IsAdmin = dto.IsAdmin,
                Avatar = "avatar.png"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create gift list for the new user
            var giftList = new GiftList
            {
                Name = $"Liste de {dto.FirstName ?? dto.Login}",
                UserId = user.Id
            };

            _context.Lists.Add(giftList);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Utilisateur créé avec succès", userId = user.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Met à jour les informations d'un utilisateur existant.
    /// </summary>
    /// <param name="id">L'ID de l'utilisateur à modifier.</param>
    /// <param name="dto">Les nouvelles informations de l'utilisateur (email, firstName, lastName, familyId, isChildren, isAdmin).</param>
    /// <returns>Un message de confirmation.</returns>
    /// <response code="200">Utilisateur mis à jour avec succès.</response>
    /// <response code="400">Tentative de modification de l'utilisateur admin système.</response>
    /// <response code="401">Non autorisé (réservé aux administrateurs).</response>
    /// <response code="404">Utilisateur non trouvé.</response>
    /// <response code="500">Erreur serveur lors de la mise à jour.</response>
    [HttpPut("users/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserAdminDto dto)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Utilisateur non trouvé" });
            }

            if (user.Id == 1)
            {
                return BadRequest(new { message = "Impossible de modifier l'utilisateur admin" });
            }

            // Update fields
            if (dto.Email != null) user.Email = dto.Email;
            if (dto.FirstName != null) user.FirstName = dto.FirstName;
            if (dto.LastName != null) user.LastName = dto.LastName;
            if (dto.FamilyId.HasValue) user.FamilyId = dto.FamilyId.Value;
            if (dto.IsChildren.HasValue) user.IsChildren = dto.IsChildren.Value;
            if (dto.IsAdmin.HasValue) user.IsAdmin = dto.IsAdmin.Value;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Utilisateur mis à jour avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Supprime un utilisateur de l'application.
    /// </summary>
    /// <param name="id">L'ID de l'utilisateur à supprimer.</param>
    /// <returns>Un message de confirmation.</returns>
    /// <response code="200">Utilisateur supprimé avec succès.</response>
    /// <response code="400">Tentative de suppression de l'utilisateur admin système.</response>
    /// <response code="401">Non autorisé (réservé aux administrateurs).</response>
    /// <response code="404">Utilisateur non trouvé.</response>
    /// <response code="500">Erreur serveur lors de la suppression.</response>
    [HttpDelete("users/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Utilisateur non trouvé" });
            }

            if (user.Id == 1)
            {
                return BadRequest(new { message = "Impossible de supprimer l'utilisateur admin" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Utilisateur supprimé avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Récupère la liste de toutes les familles avec le nombre d'utilisateurs.
    /// </summary>
    /// <returns>La liste des familles avec leur nombre d'utilisateurs.</returns>
    /// <response code="200">Liste des familles récupérée avec succès.</response>
    /// <response code="401">Non autorisé (réservé aux administrateurs).</response>
    /// <response code="500">Erreur serveur lors de la récupération des familles.</response>
    [HttpGet("families")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetAllFamilies()
    {
        try
        {
            var families = await _context.Families
                .Select(f => new
                {
                    f.Id,
                    f.Name,
                    UserCount = f.Users.Count(u => u.Id != 1),
                    f.CreatedAt
                })
                .OrderBy(f => f.Name)
                .ToListAsync();

            return Ok(families);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting families");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Crée une nouvelle famille.
    /// </summary>
    /// <param name="dto">Le nom de la nouvelle famille.</param>
    /// <returns>Un message de confirmation avec l'ID de la nouvelle famille.</returns>
    /// <response code="200">Famille créée avec succès.</response>
    /// <response code="400">Une famille avec ce nom existe déjà.</response>
    /// <response code="401">Non autorisé (réservé aux administrateurs).</response>
    /// <response code="500">Erreur serveur lors de la création.</response>
    [HttpPost("families")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CreateFamily([FromBody] CreateFamilyDto dto)
    {
        try
        {
            if (await _context.Families.AnyAsync(f => f.Name == dto.Name))
            {
                return BadRequest(new { message = "Une famille avec ce nom existe déjà" });
            }

            var family = new Family
            {
                Name = dto.Name
            };

            _context.Families.Add(family);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Famille créée avec succès", familyId = family.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating family");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Met à jour le nom d'une famille existante.
    /// </summary>
    /// <param name="id">L'ID de la famille à modifier.</param>
    /// <param name="dto">Le nouveau nom de la famille.</param>
    /// <returns>Un message de confirmation.</returns>
    /// <response code="200">Famille mise à jour avec succès.</response>
    /// <response code="401">Non autorisé (réservé aux administrateurs).</response>
    /// <response code="404">Famille non trouvée.</response>
    /// <response code="500">Erreur serveur lors de la mise à jour.</response>
    [HttpPut("families/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateFamily(int id, [FromBody] UpdateFamilyDto dto)
    {
        try
        {
            var family = await _context.Families.FindAsync(id);
            if (family == null)
            {
                return NotFound(new { message = "Famille non trouvée" });
            }

            family.Name = dto.Name;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Famille mise à jour avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating family");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Supprime une famille (uniquement si elle ne contient aucun utilisateur).
    /// </summary>
    /// <param name="id">L'ID de la famille à supprimer.</param>
    /// <returns>Un message de confirmation.</returns>
    /// <response code="200">Famille supprimée avec succès.</response>
    /// <response code="400">La famille contient des utilisateurs et ne peut pas être supprimée.</response>
    /// <response code="401">Non autorisé (réservé aux administrateurs).</response>
    /// <response code="404">Famille non trouvée.</response>
    /// <response code="500">Erreur serveur lors de la suppression.</response>
    [HttpDelete("families/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteFamily(int id)
    {
        try
        {
            var family = await _context.Families
                .Include(f => f.Users)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (family == null)
            {
                return NotFound(new { message = "Famille non trouvée" });
            }

            if (family.Users.Any(u => u.Id != 1))
            {
                return BadRequest(new { message = "Impossible de supprimer une famille qui contient des utilisateurs" });
            }

            _context.Families.Remove(family);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Famille supprimée avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting family");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}
