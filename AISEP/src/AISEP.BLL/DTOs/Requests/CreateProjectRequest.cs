using AISEP.DAL.Enums;

namespace AISEP.BLL.DTOs.Requests
{
    public class CreateProjectRequest
    {
        public string          ProjectName            { get; set; } = string.Empty;
        public IFormFile?      ProjectImageFile       { get; set; }
        public string          ShortDescription       { get; set; } = string.Empty;
        public DevelopmentStage DevelopmentStage      { get; set; }
        public string          ProblemStatement       { get; set; } = string.Empty;
        public string          SolutionDescription    { get; set; } = string.Empty;
        public string          TargetCustomers        { get; set; } = string.Empty;
        public string?         UniqueValueProposition { get; set; }
        public decimal?        MarketSize             { get; set; }
        public string?         BusinessModel          { get; set; }
        public decimal?        Revenue                { get; set; }
        public string?         Competitors            { get; set; }
        public string          TeamMembers            { get; set; } = string.Empty;
        public string?         KeySkills              { get; set; }
        public string?         TeamExperience         { get; set; }
        public Industry?       Industry               { get; set; }
    }
}
