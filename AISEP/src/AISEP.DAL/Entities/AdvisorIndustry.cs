using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class AdvisorIndustry
    {
        public int AdvisorId { get; set; }
        public Industry Industry { get; set; }

        public Advisor Advisor { get; set; } = null!;
    }
}
