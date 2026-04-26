namespace AISEP.DAL.Entities
{
    public class ProjectIndustry
    {
        public int ProjectId { get; set; }
        public int IndustryOptionId { get; set; }

        public Project Project { get; set; } = null!;
        public IndustryOption IndustryOption { get; set; } = null!;
    }
}
