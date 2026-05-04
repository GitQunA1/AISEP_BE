using System.ComponentModel.DataAnnotations;

namespace AISEP.DAL.Entities
{
    public class ScorecardWeightConfig
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        public string ConfigName { get; set; } = string.Empty;

        public decimal TeamWeight { get; set; }
        public decimal MarketWeight { get; set; }
        public decimal ProductWeight { get; set; }
        public decimal CompetitionWeight { get; set; }
        public decimal TractionWeight { get; set; }
        public decimal InvestmentNeedWeight { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
