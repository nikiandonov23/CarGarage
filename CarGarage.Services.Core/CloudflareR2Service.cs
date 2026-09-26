using Amazon.S3;
using Amazon.S3.Model;
using CarGarage.Services.Core.Contracts;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace CarGarage.Services.Core
{
    public class CloudflareR2Service : ICloudflareR2Service
    {
        private readonly IAmazonS3 _s3Client;
        private const string BucketName = "cars-images";
        private const string R2PublicUrlDomain = "https://68b155d8159aa97dd202ab672e40a804.r2.cloudflarestorage.com";

        public CloudflareR2Service()
        {
            var config = new AmazonS3Config
            {
                ServiceURL = R2PublicUrlDomain,
                ForcePathStyle = true,
                // R2 requires some region to be set, though it ignores it.
                AuthenticationRegion = "us-east-1"
            };

            const string accessKeyId = "f100c6b3480205e8b87cb08a7058e5db";
            const string secretAccessKey = "9157dfdc5fa75695444f276f1332b52734f44fb6f394b8d0b709cc00bb09ad28";

            _s3Client = new AmazonS3Client(accessKeyId, secretAccessKey, config);
        }

        public async Task<(string imageUrl, string storageKey)> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Файлът е празен.", nameof(file));
            }

            var uniqueId = Guid.NewGuid().ToString("N");
            var originalExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var storageKey = $"{uniqueId}.jpg"; // We save as JPEG for standard consistency and optimization.

            using var inputStream = file.OpenReadStream();
            using var outputStream = new MemoryStream();

            // Load and resize the image with SixLabors.ImageSharp
            using (var image = await Image.LoadAsync(inputStream))
            {
                var maxDimension = 1200;
                var width = image.Width;
                var height = image.Height;

                if (width > maxDimension || height > maxDimension)
                {
                    if (width > height)
                    {
                        height = (int)((double)height / width * maxDimension);
                        width = maxDimension;
                    }
                    else
                    {
                        width = (int)((double)width / height * maxDimension);
                        height = maxDimension;
                    }

                    image.Mutate(x => x.Resize(width, height));
                }

                // Save optimized JPEG to memory stream with 80% compression quality
                await image.SaveAsync(outputStream, new JpegEncoder
                {
                    Quality = 80
                });
            }

            outputStream.Position = 0;

            var putRequest = new PutObjectRequest
            {
                BucketName = BucketName,
                Key = storageKey,
                InputStream = outputStream,
                ContentType = "image/jpeg",
                DisablePayloadSigning = true
            };

            await _s3Client.PutObjectAsync(putRequest);

            // Construct the relative internal retrieval URL
            // This maps directly to our Controller action which streams the file from Cloudflare R2 securely.
            var imageUrl = $"/MyCars/Image/{storageKey}";

            return (imageUrl, storageKey);
        }

        public async Task<Stream?> GetImageStreamAsync(string storageKey)
        {
            if (string.IsNullOrWhiteSpace(storageKey)) return null;

            try
            {
                var response = await _s3Client.GetObjectAsync(BucketName, storageKey);
                return response.ResponseStream;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> DeleteImageAsync(string storageKey)
        {
            if (string.IsNullOrWhiteSpace(storageKey)) return false;

            try
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = BucketName,
                    Key = storageKey
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
