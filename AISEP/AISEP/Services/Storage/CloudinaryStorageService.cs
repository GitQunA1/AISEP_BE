using AISEP.Settings;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace AISEP.Services.Storage
{
    /// <summary>
    /// Implementation: Upload file lên Cloudinary.
    /// Log chi tiết nằm bên trong service con này, service cha không cần biết.
    /// </summary>
    public class CloudinaryStorageService : IStorageService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryStorageService> _logger;

        public CloudinaryStorageService(
            IOptions<CloudinarySettings> cloudinarySettings,
            ILogger<CloudinaryStorageService> logger)
        {
            _logger = logger;

            var settings = cloudinarySettings.Value;
            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder = "aisep-documents")
        {
            _logger.LogInformation("Uploading file '{FileName}' ({Size} bytes) to Cloudinary folder '{Folder}'...",
                file.FileName, file.Length, folder);

            using var stream = file.OpenReadStream();

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var errorMsg = uploadResult.Error?.Message ?? "Unknown error";
                _logger.LogError("Cloudinary upload failed for file '{FileName}': {Error}", file.FileName, errorMsg);
                throw new Exception($"Cloudinary upload failed: {errorMsg}");
            }

            _logger.LogInformation("File '{FileName}' uploaded successfully: {Url}", file.FileName, uploadResult.SecureUrl);
            return uploadResult.SecureUrl.ToString();
        }
    }
}
