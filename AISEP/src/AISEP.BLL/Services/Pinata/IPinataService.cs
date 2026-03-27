using AISEP.BLL.DTOs.Requests;

namespace AISEP.BLL.Services.Pinata
{
    public interface IPinataService
    {
        Task<string> UploadJsonToIpfsAsync(NftMetadataDto metadata);
    }
}
