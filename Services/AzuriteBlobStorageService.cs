using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace EventEaseLocal.Services
{
    public class AzuriteBlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _container;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        public AzuriteBlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage")
                ?? throw new InvalidOperationException("AzureStorage connection string is not configured.");
            var containerName = configuration["BlobStorage:VenueImagesContainer"] ?? "venue-images";

            var clientOptions = new BlobClientOptions(BlobClientOptions.ServiceVersion.V2025_01_05);
            var serviceClient = new BlobServiceClient(connectionString, clientOptions);
            _container = serviceClient.GetBlobContainerClient(containerName);
            _container.CreateIfNotExists(PublicAccessType.Blob);
        }

        public async Task<string?> UploadVenueImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext) || file.Length > MaxFileSizeBytes) return null;

            var blobName = $"{Guid.NewGuid()}{ext}";
            var blobClient = _container.GetBlobClient(blobName);

            var contentType = ext switch
            {
                ".png" => "image/png",
                _ => "image/jpeg"
            };

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });
            return blobClient.Uri.ToString();
        }

        public async Task DeleteVenueImageAsync(string? blobUrl)
        {
            if (string.IsNullOrWhiteSpace(blobUrl)) return;
            if (!Uri.TryCreate(blobUrl, UriKind.Absolute, out var uri)) return;

            var blobName = Path.GetFileName(uri.LocalPath);
            if (string.IsNullOrEmpty(blobName)) return;

            var expectedHost = _container.Uri.Host;
            if (!string.Equals(uri.Host, expectedHost, StringComparison.OrdinalIgnoreCase)) return;

            var blobClient = _container.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
