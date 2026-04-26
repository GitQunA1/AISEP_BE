namespace AISEP.DAL.Entities
{
    public class InvestorIndustry
    {
        public int InvestorId { get; set; }
        public int IndustryOptionId { get; set; }

        public Investor Investor { get; set; } = null!;
        public IndustryOption IndustryOption { get; set; } = null!;
    }
}
