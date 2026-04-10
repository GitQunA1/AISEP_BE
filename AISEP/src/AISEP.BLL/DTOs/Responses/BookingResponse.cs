using AISEP.DAL.Enums;

namespace AISEP.BLL.DTOs.Responses
{
    public class BookingResponse
    {
        public int Id { get; set; }
        public int AdvisorId { get; set; }
        public string AdvisorName { get; set; } = string.Empty;
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal Price { get; set; }
        public BookingStatus Status { get; set; }
        public string? Note { get; set; }
        public decimal SystemCommissionPercent { get; set; }
        public decimal SystemCommissionAmount { get; set; }
        public string SystemCommissionMessage { get; set; } = string.Empty;
        public int? OldBookingId { get; set; }
        public bool IsFreeRebookFromComplaint { get; set; }
        public bool IsPaymentWaived { get; set; }
        public bool UsedPremiumFreeQuota { get; set; }
        public bool PremiumFreeQuotaRefunded { get; set; }
        public List<int> AdvisorAvailabilitySlotIds { get; set; } = [];
        public int SlotCount { get; set; }
    }
}
