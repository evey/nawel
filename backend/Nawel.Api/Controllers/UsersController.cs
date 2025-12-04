using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nawel.Api.Data;
using Nawel.Api.DTOs;
using Nawel.Api.Services.Auth;
using System.Security.Claims;

namespace Nawel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly NawelDbContext _context;
    private readonly IAuthService _authService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        NawelDbContext context,
        IAuthService authService,
        ILogger<UsersController> logger)
    {
        _context = context;
        _authService = authService;
        _logger = logger;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var user = await _context.Users
                .Include(u => u.Family)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Login = user.Login,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Avatar = user.Avatar,
                Pseudo = user.Pseudo,
                NotifyListEdit = user.NotifyListEdit,
                NotifyGiftTaken = user.NotifyGiftTaken,
                DisplayPopup = user.DisplayPopup,
                IsChildren = user.IsChildren,
                FamilyId = user.FamilyId,
                FamilyName = user.Family?.Name
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateCurrentUser([FromBody] UpdateUserDto updateDto)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var user = await _context.Users
                .Include(u => u.Family)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Update only provided fields
            if (updateDto.Email != null) user.Email = updateDto.Email;
            if (updateDto.FirstName != null) user.FirstName = updateDto.FirstName;
            if (updateDto.LastName != null) user.LastName = updateDto.LastName;
            if (updateDto.Pseudo != null) user.Pseudo = updateDto.Pseudo;
            if (updateDto.NotifyListEdit.HasValue) user.NotifyListEdit = updateDto.NotifyListEdit.Value;
            if (updateDto.NotifyGiftTaken.HasValue) user.NotifyGiftTaken = updateDto.NotifyGiftTaken.Value;
            if (updateDto.DisplayPopup.HasValue) user.DisplayPopup = updateDto.DisplayPopup.Value;

            await _context.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = user.Id,
                Login = user.Login,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Avatar = user.Avatar,
                Pseudo = user.Pseudo,
                NotifyListEdit = user.NotifyListEdit,
                NotifyGiftTaken = user.NotifyGiftTaken,
                DisplayPopup = user.DisplayPopup,
                IsChildren = user.IsChildren,
                FamilyId = user.FamilyId,
                FamilyName = user.Family?.Name
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating current user");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPost("me/change-password")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var login = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(login))
            {
                return BadRequest(new { message = "Invalid user" });
            }

            // Verify current password
            var user = await _authService.AuthenticateAsync(login, changePasswordDto.CurrentPassword);
            if (user == null)
            {
                return BadRequest(new { message = "Current password is incorrect" });
            }

            // Update password
            var success = await _authService.UpdatePasswordAsync(currentUserId, changePasswordDto.NewPassword);
            if (!success)
            {
                return BadRequest(new { message = "Failed to update password" });
            }

            return Ok(new { message = "Password updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}
