using Nawel.Api.Constants;

namespace Nawel.Api.Configuration;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "NawelApi";
    public string Audience { get; set; } = "NawelApp";
    public int ExpirationMinutes { get; set; } = ApplicationConstants.Authentication.DefaultTokenExpirationMinutes;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Secret))
        {
            throw new InvalidOperationException(
                "JWT Secret must be configured via JWT_SECRET environment variable or Jwt:Secret in appsettings.json");
        }

        if (Secret.Length < ApplicationConstants.Authentication.MinJwtSecretLength)
        {
            throw new InvalidOperationException(
                $"JWT Secret must be at least {ApplicationConstants.Authentication.MinJwtSecretLength} characters long for security");
        }

        if (string.IsNullOrWhiteSpace(Issuer))
        {
            throw new InvalidOperationException("JWT Issuer must be configured");
        }

        if (string.IsNullOrWhiteSpace(Audience))
        {
            throw new InvalidOperationException("JWT Audience must be configured");
        }
    }
}
