using AISEP.Models.Enums;

namespace AISEP.DTOs.Requests
{
    public class UploadDocumentRequest
    {
        public IFormFile File { get; set; } = null!;
        public int StartupId { get; set; }
        public bool IsIpProtected { get; set; }
        public DocumentType DocumentType { get; set; }
    }
}
