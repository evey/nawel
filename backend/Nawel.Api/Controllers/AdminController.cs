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

    private bool IsAdmin()
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        return currentUserId == 1; // User ID 1 is admin
    }

    // GET: api/admin/stats
    [HttpGet("stats")]
    public async Task<ActionResult> GetStats()
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

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

    // GET: api/admin/users
    [HttpGet("users")]
    public async Task<ActionResult> GetAllUsers()
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

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

    // POST: api/admin/users
    [HttpPost("users")]
    public async Task<ActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

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

    // PUT: api/admin/users/{id}
    [HttpPut("users/{id}")]
    public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserAdminDto dto)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

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

            await _context.SaveChangesAsync();

            return Ok(new { message = "Utilisateur mis à jour avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    // DELETE: api/admin/users/{id}
    [HttpDelete("users/{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

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

    // GET: api/admin/families
    [HttpGet("families")]
    public async Task<ActionResult> GetAllFamilies()
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

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

    // POST: api/admin/families
    [HttpPost("families")]
    public async Task<ActionResult> CreateFamily([FromBody] CreateFamilyDto dto)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

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

    // PUT: api/admin/families/{id}
    [HttpPut("families/{id}")]
    public async Task<ActionResult> UpdateFamily(int id, [FromBody] UpdateFamilyDto dto)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

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

    // DELETE: api/admin/families/{id}
    [HttpDelete("families/{id}")]
    public async Task<ActionResult> DeleteFamily(int id)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

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
