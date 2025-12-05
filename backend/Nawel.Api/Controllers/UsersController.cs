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
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public UsersController(
        NawelDbContext context,
        IAuthService authService,
        ILogger<UsersController> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _context = context;
        _authService = authService;
        _logger = logger;
        _configuration = configuration;
        _environment = environment;
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

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(int id)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Family)
                .FirstOrDefaultAsync(u => u.Id == id);

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
            _logger.LogError(ex, "Error getting user by id");
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

    [HttpPost("me/avatar")]
    public async Task<ActionResult> UploadAvatar()
    {
        try
        {
            _logger.LogInformation("UploadAvatar called");
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation("User ID: {UserId}", currentUserId);

            // Log form data
            _logger.LogInformation("Request HasFormContentType: {HasForm}", Request.HasFormContentType);
            _logger.LogInformation("Request ContentType: {ContentType}", Request.ContentType);
            _logger.LogInformation("Request ContentLength: {ContentLength}", Request.ContentLength);

            var file = Request.Form.Files.GetFile("file");

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("No file provided or file is empty. File is null: {IsNull}, Length: {Length}",
                    file == null, file?.Length ?? 0);
                _logger.LogWarning("Form files count: {Count}", Request.Form.Files.Count);
                if (Request.Form.Files.Count > 0)
                {
                    _logger.LogWarning("Available file names: {Names}", string.Join(", ", Request.Form.Files.Select(f => f.Name)));
                }
                return BadRequest(new { message = "No file provided" });
            }

            _logger.LogInformation("File received: {FileName}, Size: {FileSize} bytes", file.FileName, file.Length);

            // Get configuration
            var maxFileSizeMB = _configuration.GetValue<int>("FileStorage:MaxFileSizeMB", 5);
            var allowedExtensions = _configuration.GetSection("FileStorage:AllowedExtensions").Get<string[]>()
                ?? new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var avatarsPath = _configuration.GetValue<string>("FileStorage:AvatarsPath", "uploads/avatars");

            // Validate file size
            var maxFileSizeBytes = maxFileSizeMB * 1024 * 1024;
            if (file.Length > maxFileSizeBytes)
            {
                return BadRequest(new { message = $"File size exceeds maximum allowed size of {maxFileSizeMB}MB" });
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = $"File type not allowed. Allowed types: {string.Join(", ", allowedExtensions)}" });
            }

            // Get user from database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Create uploads directory if it doesn't exist
            var uploadsDir = Path.Combine(_environment.ContentRootPath, avatarsPath);
            Directory.CreateDirectory(uploadsDir);

            // Delete old avatar if exists
            if (!string.IsNullOrEmpty(user.Avatar))
            {
                var oldAvatarPath = Path.Combine(_environment.ContentRootPath, user.Avatar);
                if (System.IO.File.Exists(oldAvatarPath))
                {
                    try
                    {
                        System.IO.File.Delete(oldAvatarPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old avatar file: {Path}", oldAvatarPath);
                    }
                }
            }

            // Generate unique filename
            var fileName = $"user_{currentUserId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update user avatar path (relative path for database)
            var relativeAvatarPath = Path.Combine(avatarsPath, fileName).Replace("\\", "/");
            user.Avatar = relativeAvatarPath;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Avatar uploaded successfully for user {UserId}: {AvatarPath}", currentUserId, relativeAvatarPath);

            return Ok(new {
                message = "Avatar uploaded successfully",
                avatarUrl = $"/{relativeAvatarPath}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading avatar");
            return StatusCode(500, new { message = "An error occurred while uploading the avatar" });
        }
    }

    [HttpDelete("me/avatar")]
    public async Task<ActionResult> DeleteAvatar()
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Delete avatar file if exists
            if (!string.IsNullOrEmpty(user.Avatar))
            {
                var avatarPath = Path.Combine(_environment.ContentRootPath, user.Avatar);
                if (System.IO.File.Exists(avatarPath))
                {
                    try
                    {
                        System.IO.File.Delete(avatarPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete avatar file: {Path}", avatarPath);
                    }
                }

                user.Avatar = null;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Avatar deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting avatar");
            return StatusCode(500, new { message = "An error occurred while deleting the avatar" });
        }
    }
}
