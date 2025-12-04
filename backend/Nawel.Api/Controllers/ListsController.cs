using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nawel.Api.Data;
using Nawel.Api.DTOs;
using System.Security.Claims;

namespace Nawel.Api.Controllers;

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

    [HttpGet]
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
                            GiftCount = u.List?.Gifts.Count(g => g.Year == currentYear) ?? 0
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

    [HttpGet("mine")]
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
