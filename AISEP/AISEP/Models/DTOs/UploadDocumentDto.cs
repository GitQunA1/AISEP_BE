using AISEP.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace AISEP.Models.DTOs
{
    /// <summary>
    /// Request DTO để upload Document mới.
    /// </summary>
    public class UploadDocumentDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        [Required]
        public int ProjectId { get; set; }

        [Required]
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// Có muốn bảo vệ IP (lưu hash lên Blockchain) không?
        /// Mặc định = false → chỉ upload Cloudinary, không ghi Blockchain.
        /// </summary>
        public bool IsIpProtected { get; set; } = false;
    }
}
