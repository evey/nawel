using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nawel.Api.Data;
using Nawel.Api.DTOs;
using Nawel.Api.Extensions;
using Nawel.Api.Models;
using Nawel.Api.Services.Auth;
using System.Security.Claims;

namespace Nawel.Api.Controllers;

/// <summary>
/// Contrôleur pour la gestion des profils utilisateurs (consultation, modification, avatar, mot de passe).
/// </summary>
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

    /// <summary>
    /// Récupère les informations de l'utilisateur actuellement connecté.
    /// </summary>
    /// <returns>Les informations complètes de l'utilisateur (profil, famille, préférences).</returns>
    /// <response code="200">Utilisateur récupéré avec succès.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="404">Utilisateur non trouvé.</response>
    /// <response code="500">Erreur serveur lors de la récupération.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

            return Ok(user.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Récupère les informations d'un utilisateur spécifique par son ID.
    /// </summary>
    /// <param name="id">L'ID de l'utilisateur.</param>
    /// <returns>Les informations complètes de l'utilisateur.</returns>
    /// <response code="200">Utilisateur récupéré avec succès.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="404">Utilisateur non trouvé.</response>
    /// <response code="500">Erreur serveur lors de la récupération.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

            return Ok(user.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by id");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Met à jour les informations du profil de l'utilisateur connecté.
    /// </summary>
    /// <param name="updateDto">Les nouvelles informations (email, firstName, lastName, pseudo, préférences de notification).</param>
    /// <returns>Les informations mises à jour de l'utilisateur.</returns>
    /// <response code="200">Profil mis à jour avec succès.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="404">Utilisateur non trouvé.</response>
    /// <response code="500">Erreur serveur lors de la mise à jour.</response>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

            return Ok(user.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating current user");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Change le mot de passe de l'utilisateur connecté.
    /// </summary>
    /// <param name="changePasswordDto">Le mot de passe actuel et le nouveau mot de passe.</param>
    /// <returns>Un message de confirmation si le changement est réussi.</returns>
    /// <response code="200">Mot de passe changé avec succès.</response>
    /// <response code="400">Mot de passe actuel incorrect ou échec de la mise à jour.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="500">Erreur serveur lors du changement de mot de passe.</response>
    [HttpPost("me/change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Upload un nouvel avatar pour l'utilisateur connecté.
    /// </summary>
    /// <returns>L'URL du nouvel avatar.</returns>
    /// <response code="200">Avatar uploadé avec succès.</response>
    /// <response code="400">Fichier invalide (taille, format, absence de fichier).</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="404">Utilisateur non trouvé.</response>
    /// <response code="500">Erreur serveur lors de l'upload.</response>
    /// <remarks>
    /// Le fichier doit être envoyé en multipart/form-data avec la clé "file".
    /// Formats acceptés : .jpg, .jpeg, .png, .gif, .webp.
    /// Taille maximale configurable (défaut : 5MB).
    /// </remarks>
    [HttpPost("me/avatar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

            // Get configuration
            var maxFileSizeMB = _configuration.GetValue<int>("FileStorage:MaxFileSizeMB", 5);
            var allowedExtensions = _configuration.GetSection("FileStorage:AllowedExtensions").Get<string[]>()
                ?? new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var avatarsPath = _configuration.GetValue<string>("FileStorage:AvatarsPath", "uploads/avatars");

            // Validate file
            var validationError = ValidateUploadedFile(file, maxFileSizeMB, allowedExtensions);
            if (validationError != null)
            {
                return validationError;
            }

            var extension = Path.GetExtension(file!.FileName).ToLowerInvariant();

            // Get user from database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Delete old avatar
            DeleteOldAvatar(user);

            // Save new avatar file
            var fileName = await SaveAvatarFile(file, currentUserId, avatarsPath, extension);

            // Update user avatar in database
            var relativeAvatarPath = await UpdateUserAvatar(user, fileName, avatarsPath);

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

    /// <summary>
    /// Supprime l'avatar de l'utilisateur connecté et restaure l'avatar par défaut.
    /// </summary>
    /// <returns>Un message de confirmation.</returns>
    /// <response code="200">Avatar supprimé avec succès et remplacé par l'avatar par défaut (avatar.png).</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="404">Utilisateur non trouvé.</response>
    /// <response code="500">Erreur serveur lors de la suppression.</response>
    [HttpDelete("me/avatar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

                user.Avatar = "avatar.png"; // Reset to default avatar
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

    // Private helper methods for UploadAvatar
    private ActionResult? ValidateUploadedFile(IFormFile? file, int maxFileSizeMB, string[] allowedExtensions)
    {
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

        return null; // No validation errors
    }

    private void DeleteOldAvatar(User user)
    {
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
    }

    private async Task<string> SaveAvatarFile(IFormFile file, int userId, string avatarsPath, string extension)
    {
        var uploadsDir = Path.Combine(_environment.ContentRootPath, avatarsPath);
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"user_{userId}_{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return fileName;
    }

    private async Task<string> UpdateUserAvatar(User user, string fileName, string avatarsPath)
    {
        var relativeAvatarPath = Path.Combine(avatarsPath, fileName).Replace("\\", "/");
        user.Avatar = relativeAvatarPath;
        await _context.SaveChangesAsync();

        return relativeAvatarPath;
    }
}
