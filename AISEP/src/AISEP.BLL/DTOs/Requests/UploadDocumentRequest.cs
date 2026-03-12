using AISEP.DAL.Enums;

namespace AISEP.BLL.DTOs.Requests
{
    public class UploadDocumentRequest
    {
        public DocumentType DocumentType { get; set; }
        public IFormFile    File         { get; set; } = null!;
    }
}
