using AISEP.DAL.Enums;

namespace AISEP.BLL.DTOs.Responses
{
    public class SubscriptionResponseDto
    {
        public int SubscriptionId { get; set; }
        public int PackageId { get; set; }
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public SubscriptionStatus Status { get; set; }
        public int UsedAiRequests { get; set; }
        public int UsedProjectViews { get; set; }
        public int RemainingFreeBookings { get; set; }

        public string PackageName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }
}
