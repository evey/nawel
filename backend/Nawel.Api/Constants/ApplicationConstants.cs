namespace Nawel.Api.Constants;

public static class ApplicationConstants
{
    /// <summary>
    /// File upload related constants
    /// </summary>
    public static class FileUpload
    {
        /// <summary>
        /// Maximum file upload size in bytes (10 MB)
        /// </summary>
        public const long MaxFileSizeBytes = 10 * 1024 * 1024;

        /// <summary>
        /// Maximum avatar file size in bytes (5 MB)
        /// </summary>
        public const long MaxAvatarSizeBytes = 5 * 1024 * 1024;

        /// <summary>
        /// Allowed avatar file extensions
        /// </summary>
        public static readonly string[] AllowedAvatarExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    }

    /// <summary>
    /// Authentication and security related constants
    /// </summary>
    public static class Authentication
    {
        /// <summary>
        /// Minimum JWT secret length in characters
        /// </summary>
        public const int MinJwtSecretLength = 32;

        /// <summary>
        /// Default token expiration in minutes
        /// </summary>
        public const int DefaultTokenExpirationMinutes = 60;

        /// <summary>
        /// Password reset token expiration in hours
        /// </summary>
        public const int PasswordResetTokenExpirationHours = 1;

        /// <summary>
        /// Reset token length in bytes
        /// </summary>
        public const int ResetTokenLengthBytes = 32;
    }

    /// <summary>
    /// Rate limiting related constants
    /// </summary>
    public static class RateLimiting
    {
        public const int LoginAttemptsPerMinute = 5;
        public const int LoginAttemptsPerFifteenMinutes = 10;
        public const int ForgotPasswordAttemptsPerHour = 3;
        public const int ResetPasswordAttemptsPerHour = 5;
        public const int GeneralAuthAttemptsPerMinute = 20;
    }

    /// <summary>
    /// Email notification related constants
    /// </summary>
    public static class Email
    {
        /// <summary>
        /// Default notification delay in minutes
        /// </summary>
        public const int DefaultNotificationDelayMinutes = 2;

        /// <summary>
        /// Default reservation notification delay in minutes
        /// </summary>
        public const int DefaultReservationDelayMinutes = 2;
    }

    /// <summary>
    /// Database and data related constants
    /// </summary>
    public static class Data
    {
        /// <summary>
        /// Default page size for paginated results
        /// </summary>
        public const int DefaultPageSize = 50;

        /// <summary>
        /// Maximum description length
        /// </summary>
        public const int MaxDescriptionLength = 500;
    }
}
