using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class MonthlyPayoutBatch
    {
        public int MonthlyPayoutBatchId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal EstimatedTotalAmount { get; set; }
        public decimal RejectedAmount { get; set; }
        public decimal ActualPayableAmount { get; set; }
        public MonthlyPayoutBatchStatus Status { get; set; } = MonthlyPayoutBatchStatus.InProgress;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public ICollection<MonthlyPayout> MonthlyPayouts { get; set; } = new List<MonthlyPayout>();
    }
}
