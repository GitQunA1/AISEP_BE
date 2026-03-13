using AISEP.BLL.Settings;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

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

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams, "auto");

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var errorMsg = uploadResult.Error?.Message ?? "Unknown error";
                throw new Exception($"Cloudinary upload failed: {errorMsg}");
            }

            return uploadResult.SecureUrl.ToString();
        }
    }
}
