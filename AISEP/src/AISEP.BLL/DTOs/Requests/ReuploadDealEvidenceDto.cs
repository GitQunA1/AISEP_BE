using Microsoft.AspNetCore.Http;

namespace AISEP.BLL.DTOs.Requests
{
    public class ReuploadDealEvidenceDto
    {
        public IFormFile EvidenceFile { get; set; } = null!;
    }
}
