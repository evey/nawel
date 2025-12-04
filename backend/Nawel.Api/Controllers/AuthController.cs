using Microsoft.AspNetCore.Mvc;
using Nawel.Api.DTOs;
using Nawel.Api.Services.Auth;

namespace Nawel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IJwtService jwtService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("login")]
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

            return Ok(new LoginResponse
            {
                Token = token,
                User = userDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Login}", request.Login);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    [HttpPost("reset-password-request")]
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

    [HttpPost("reset-password")]
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

    [HttpGet("validate-token")]
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
