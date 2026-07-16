using Microsoft.Extensions.Logging;
using ProductApi.Exceptions;

namespace ProductApi.Services;

public class ImageUploadService : IImageUploadService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private const string UploadSubPath = "uploads/events";

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp",
    };

    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ImageUploadService> _logger;

    public ImageUploadService(IWebHostEnvironment environment, ILogger<ImageUploadService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> SaveEventImageAsync(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            _logger.LogWarning("Image upload rejected: no file or empty file provided.");
            throw new ImageValidationException("Image file is required.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            _logger.LogWarning(
                "Image upload rejected: file '{FileName}' is {Size} bytes, exceeds the 5 MB limit.",
                file.FileName,
                file.Length);
            throw new ImageValidationException("Image exceeds the maximum allowed size of 5 MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            _logger.LogWarning(
                "Image upload rejected: file '{FileName}' has disallowed extension '{Extension}'.",
                file.FileName,
                extension);
            throw new ImageValidationException(
                $"File extension '{extension}' is not allowed. Allowed extensions: .jpg, .jpeg, .png, .webp.");
        }

        await using var inputStream = file.OpenReadStream();
        var header = new byte[12];
        var bytesRead = await inputStream.ReadAsync(header.AsMemory(0, header.Length));

        if (!MatchesSignature(extension, header, bytesRead))
        {
            _logger.LogWarning(
                "Image upload rejected: file '{FileName}' claims extension '{Extension}' but its content signature does not match. Possible spoofed upload.",
                file.FileName,
                extension);
            throw new ImageValidationException("File content does not match a valid image for its extension.");
        }

        inputStream.Position = 0;

        var uploadsRoot = Path.Combine(GetWebRootPath(), "uploads", "events");
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsRoot, fileName);

        await using var outputStream = new FileStream(filePath, FileMode.Create);
        await inputStream.CopyToAsync(outputStream);

        _logger.LogInformation(
            "Image upload saved: original name '{OriginalFileName}' stored as '{StoredFileName}' ({Size} bytes).",
            file.FileName,
            fileName,
            file.Length);

        return $"/{UploadSubPath}/{fileName}";
    }

    public void DeleteEventImage(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return;
        }

        var fileName = Path.GetFileName(imageUrl);
        var filePath = Path.Combine(GetWebRootPath(), "uploads", "events", fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("Deleted event image '{StoredFileName}'.", fileName);
        }
    }

    private string GetWebRootPath() =>
        string.IsNullOrEmpty(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;

    private static bool MatchesSignature(string extension, byte[] header, int length)
    {
        return extension switch
        {
            ".jpg" or ".jpeg" => length >= 3
                && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => length >= 8
                && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A,
            ".webp" => length >= 12
                && header[0] == 'R' && header[1] == 'I' && header[2] == 'F' && header[3] == 'F'
                && header[8] == 'W' && header[9] == 'E' && header[10] == 'B' && header[11] == 'P',
            _ => false,
        };
    }
}
