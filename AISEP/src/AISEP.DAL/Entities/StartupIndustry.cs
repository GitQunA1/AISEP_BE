namespace AISEP.DAL.Entities
{
    public class StartupIndustry
    {
        public int StartupId { get; set; }
        public int IndustryOptionId { get; set; }

        public Startup Startup { get; set; } = null!;
        public IndustryOption IndustryOption { get; set; } = null!;
    }
}
