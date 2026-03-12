namespace AISEP.BLL.Services.Storage
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder = "aisep-documents");
    }
}
