using AISEP.BLL.Settings;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using System.Net;

namespace AISEP.BLL.Services.Storage
{
    public class CloudinaryStorageService : IStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryStorageService(IOptions<CloudinarySettings> cloudinarySettings)
        {
            var settings = cloudinarySettings.Value;
            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder = "aisep-documents")
        {
            using var stream = file.OpenReadStream();
            var contentType = file.ContentType?.Trim().ToLowerInvariant() ?? string.Empty;
            var fileExtension = Path.GetExtension(file.FileName).Trim().ToLowerInvariant();

            var isImage = contentType.StartsWith("image/") || fileExtension is ".jpg" or ".jpeg" or ".png" or ".webp";

            if (isImage)
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folder
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                EnsureUploadSucceeded(uploadResult.StatusCode, uploadResult.Error?.Message);
                return uploadResult.SecureUrl?.ToString()
                    ?? throw new Exception("Cloudinary upload failed: missing secure URL for uploaded image.");
            }

            var rawUploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder
            };

            var rawUploadResult = await _cloudinary.UploadAsync(rawUploadParams);
            EnsureUploadSucceeded(rawUploadResult.StatusCode, rawUploadResult.Error?.Message);
            return rawUploadResult.SecureUrl?.ToString()
                ?? throw new Exception("Cloudinary upload failed: missing secure URL for uploaded file.");
        }

        private static void EnsureUploadSucceeded(HttpStatusCode statusCode, string? errorMessage)
        {
            if (statusCode == HttpStatusCode.OK)
            {
                return;
            }

            throw new Exception($"Cloudinary upload failed: {errorMessage ?? "Unknown error"}");
        }
    }
}
