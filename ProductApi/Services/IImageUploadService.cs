using Microsoft.AspNetCore.Http;

namespace ProductApi.Services;

public interface IImageUploadService
{
    Task<string> SaveEventImageAsync(IFormFile file);

    void DeleteEventImage(string? imageUrl);
}
