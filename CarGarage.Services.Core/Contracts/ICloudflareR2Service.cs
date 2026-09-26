using Microsoft.AspNetCore.Http;

namespace CarGarage.Services.Core.Contracts
{
    public interface ICloudflareR2Service
    {
        Task<(string imageUrl, string storageKey)> UploadImageAsync(IFormFile file, string bucketName = "cars-images");
        Task<bool> DeleteImageAsync(string storageKey, string bucketName = "cars-images");
        Task<Stream?> GetImageStreamAsync(string storageKey, string bucketName = "cars-images");
    }
}
