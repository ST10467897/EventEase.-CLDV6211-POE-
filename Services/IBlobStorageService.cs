using Microsoft.AspNetCore.Http;

namespace EventEaseLocal.Services
{
    public interface IBlobStorageService
    {
        Task<string?> UploadVenueImageAsync(IFormFile file);
        Task DeleteVenueImageAsync(string? blobUrl);
    }
}
