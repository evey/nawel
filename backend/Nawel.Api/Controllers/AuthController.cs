using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nawel.Api.Data;
using Nawel.Api.DTOs;
using Nawel.Api.Exceptions;
using Nawel.Api.Extensions;
using Nawel.Api.Services.Auth;
using Nawel.Api.Services.Email;

namespace Nawel.Api.Controllers;

/// <summary>
/// Contrôleur pour la gestion de l'authentification et des réinitialisations de mots de passe.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;
    private readonly NawelDbContext _context;
    private readonly IEmailService _emailService;

    public AuthController(
        IAuthService authService,
        IJwtService jwtService,
        ILogger<AuthController> logger,
        NawelDbContext context,
        IEmailService emailService)
    {
        _authService = authService;
        _jwtService = jwtService;
        _logger = logger;
        _context = context;
        _emailService = emailService;
    }

    /// <summary>
    /// Authentifie un utilisateur avec ses identifiants.
    /// </summary>
    /// <param name="request">Les identifiants de connexion (login et mot de passe).</param>
    /// <returns>Un token JWT et les informations de l'utilisateur en cas de succès.</returns>
    /// <response code="200">Authentification réussie. Retourne le token JWT et les données utilisateur.</response>
    /// <response code="401">Identifiants invalides ou mot de passe MD5 nécessitant une migration (code LEGACY_PASSWORD).</response>
    /// <response code="500">Erreur serveur lors de l'authentification.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _authService.AuthenticateAsync(request.Login, request.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new LoginResponse
            {
                Token = token,
                User = user.ToDto()
            });
        }
        catch (LegacyPasswordException ex)
        {
            _logger.LogInformation(
                "User {Login} (ID: {UserId}) attempted login with legacy MD5 password. Returning migration required response.",
                ex.Login, ex.UserId);

            return Unauthorized(new
            {
                code = "LEGACY_PASSWORD",
                message = "Votre mot de passe doit être réinitialisé pour des raisons de sécurité",
                email = ex.Email,
                requiresReset = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Login}", request.Login);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Demande une réinitialisation de mot de passe par email.
    /// </summary>
    /// <param name="request">L'adresse email de l'utilisateur.</param>
    /// <returns>Un message de confirmation générique (pour ne pas révéler si l'email existe).</returns>
    /// <response code="200">Demande traitée. Un email est envoyé si l'adresse existe.</response>
    /// <response code="500">Erreur serveur lors du traitement de la demande.</response>
    [HttpPost("reset-password-request")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RequestPasswordReset([FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            var token = await _authService.GenerateResetTokenAsync(request.Email);

            // TODO: Send email with reset token
            _logger.LogInformation("Password reset requested for email {Email}. Token: {Token}", request.Email, token);

            return Ok(new { message = "If this email exists, a reset link has been sent" });
        }
        catch (InvalidOperationException)
        {
            // Don't reveal if user exists or not
            return Ok(new { message = "If this email exists, a reset link has been sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset request for email {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Réinitialise le mot de passe d'un utilisateur avec un token de réinitialisation valide.
    /// </summary>
    /// <param name="request">Le token de réinitialisation et le nouveau mot de passe.</param>
    /// <returns>Un message de confirmation si la réinitialisation est réussie.</returns>
    /// <response code="200">Mot de passe réinitialisé avec succès.</response>
    /// <response code="400">Token invalide, expiré ou échec de la réinitialisation.</response>
    /// <response code="500">Erreur serveur lors de la réinitialisation.</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        try
        {
            var isValid = await _authService.ValidateResetTokenAsync(request.Token);
            if (!isValid)
            {
                return BadRequest(new { message = "Invalid or expired token" });
            }

            var success = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);
            if (!success)
            {
                return BadRequest(new { message = "Failed to reset password" });
            }

            return Ok(new { message = "Password reset successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Demande une migration de mot de passe MD5 vers BCrypt. Envoie un email de réinitialisation si le compte utilise encore MD5.
    /// </summary>
    /// <param name="request">Le login de l'utilisateur.</param>
    /// <returns>Un message de confirmation générique (pour ne pas révéler si le compte nécessite une migration).</returns>
    /// <response code="200">Demande traitée. Un email est envoyé si le compte nécessite une migration.</response>
    /// <response code="500">Erreur serveur lors du traitement de la demande.</response>
    /// <remarks>
    /// Cette méthode vérifie si l'utilisateur a un mot de passe MD5 (32 caractères hexadécimaux).
    /// Si c'est le cas, un email de migration est envoyé avec un lien de réinitialisation.
    /// </remarks>
    [HttpPost("request-migration-reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RequestMigrationReset([FromBody] RequestMigrationResetDto request)
    {
        try
        {
            // Vérifier que l'utilisateur existe
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == request.Login);

            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                // Ne pas révéler si l'utilisateur existe (sécurité)
                _logger.LogInformation("Migration reset requested for non-existent or email-less login: {Login}", request.Login);
                return Ok(new { message = "Si votre compte nécessite une migration, un email a été envoyé" });
            }

            // Vérifier que c'est bien un mot de passe MD5 (32 hex chars, pas BCrypt)
            if (user.Password.Length == 32 && !user.Password.StartsWith("$2"))
            {
                // Générer le token de reset
                var token = await _authService.GenerateResetTokenAsync(user.Email);

                // Envoyer l'email de migration
                await _emailService.SendMigrationResetEmailAsync(user.Email, user.FirstName ?? user.Login, token);

                _logger.LogInformation(
                    "Migration reset email sent for user {Login} (ID: {UserId}) with legacy MD5 password",
                    user.Login, user.Id);
            }
            else
            {
                _logger.LogInformation(
                    "Migration reset requested for user {Login} (ID: {UserId}) but password is already BCrypt",
                    user.Login, user.Id);
            }

            // Toujours retourner le même message (sécurité)
            return Ok(new { message = "Si votre compte nécessite une migration, un email a été envoyé" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during migration reset request for login {Login}", request.Login);
            return StatusCode(500, new { message = "Une erreur est survenue" });
        }
    }

    /// <summary>
    /// Valide un token JWT et retourne l'ID utilisateur associé.
    /// </summary>
    /// <param name="token">Le token JWT à valider.</param>
    /// <returns>L'ID utilisateur et le statut de validation si le token est valide.</returns>
    /// <response code="200">Token valide. Retourne l'ID utilisateur.</response>
    /// <response code="401">Token invalide ou expiré.</response>
    [HttpGet("validate-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult ValidateToken([FromQuery] string token)
    {
        var userId = _jwtService.ValidateToken(token);
        if (userId == null)
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        return Ok(new { userId, valid = true });
    }
}
