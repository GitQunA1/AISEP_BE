namespace AISEP.BLL.DTOs.Responses
{
    public class PayoutGroupResponse
    {
        public int PayoutGroupId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal EstimatedTotalAmount { get; set; }
        public decimal RejectedAmount { get; set; }
        public decimal ActualPayableAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TotalBillCount { get; set; }
        public int PendingBillCount { get; set; }
        public int ApprovedBillCount { get; set; }
        public int RejectedBillCount { get; set; }
    }
}


