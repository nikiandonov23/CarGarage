using Microsoft.AspNetCore.Http;

namespace CarGarage.Services.Core.Contracts
{
    public interface ICloudflareR2Service
    {
        Task<(string imageUrl, string storageKey)> UploadImageAsync(IFormFile file);
        Task<bool> DeleteImageAsync(string storageKey);
        Task<Stream?> GetImageStreamAsync(string storageKey);
    }
}
