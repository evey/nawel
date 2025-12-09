namespace Nawel.Api.Configuration;

public class FileStorageSettings
{
    public const string SectionName = "FileStorage";

    public string AvatarsPath { get; set; } = "uploads/avatars";
    public int MaxFileSizeMB { get; set; } = 5;
    public string[] AllowedExtensions { get; set; } = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public long MaxFileSizeBytes => MaxFileSizeMB * 1024 * 1024;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(AvatarsPath))
        {
            throw new InvalidOperationException("FileStorage AvatarsPath must be configured");
        }

        if (MaxFileSizeMB <= 0)
        {
            throw new InvalidOperationException("FileStorage MaxFileSizeMB must be greater than 0");
        }

        if (AllowedExtensions == null || AllowedExtensions.Length == 0)
        {
            throw new InvalidOperationException("FileStorage AllowedExtensions must contain at least one extension");
        }

        // Ensure all extensions start with a dot
        for (int i = 0; i < AllowedExtensions.Length; i++)
        {
            if (!AllowedExtensions[i].StartsWith("."))
            {
                AllowedExtensions[i] = "." + AllowedExtensions[i];
            }
        }
    }

    public bool IsExtensionAllowed(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return false;
        }

        var normalizedExtension = extension.ToLowerInvariant();
        if (!normalizedExtension.StartsWith("."))
        {
            normalizedExtension = "." + normalizedExtension;
        }

        return AllowedExtensions.Any(ext => ext.Equals(normalizedExtension, StringComparison.OrdinalIgnoreCase));
    }
}
