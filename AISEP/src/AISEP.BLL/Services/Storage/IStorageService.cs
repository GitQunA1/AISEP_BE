namespace AISEP.BLL.Services.Storage
{
    /// <summary>
    /// Chuyên nhận file và upload lên Cloudinary.
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Upload file lên Cloudinary và trả về Secure URL.
        /// </summary>
        Task<string> UploadFileAsync(IFormFile file, string folder = "aisep-documents");
    }
}
