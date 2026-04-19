using AISEP.DAL.Enums;

namespace AISEP.BLL.DTOs.Requests
{
    public class CreateUserReportRequest
    {
        public int BookingId { get; set; }
        public UserReportCategory Category { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<IFormFile>? EvidenceImages { get; set; }
        public string? VideoEvidenceUrl { get; set; }
    }
}
