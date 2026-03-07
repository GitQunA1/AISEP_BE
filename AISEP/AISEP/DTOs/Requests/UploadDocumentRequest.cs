using AISEP.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace AISEP.DTOs.Requests
{
    public class UploadDocumentRequest
    {
        [Required]
        public DocumentType DocumentType { get; set; }

        [Required]
        public IFormFile File { get; set; } = null!;
    }
}
