namespace AISEP.BLL.DTOs.Requests
{
    public class CreateProjectRequest
    {
        public string?    ProjectName            { get; set; }
        public IFormFile? ProjectImageFile       { get; set; }
        public string?    ShortDescription       { get; set; }
        public int        StageOptionId          { get; set; }
        public string?    ProblemStatement       { get; set; }
        public string?    SolutionDescription    { get; set; }
        public string?    TargetCustomers        { get; set; }
        public string?    UniqueValueProposition { get; set; }
        public decimal?   MarketSize             { get; set; }
        public string?    BusinessModel          { get; set; }
        public decimal?   Revenue                { get; set; }
        public string?    Competitors            { get; set; }
        public string?    TeamMembers            { get; set; }
        public string?    KeySkills              { get; set; }
        public string?    TeamExperience         { get; set; }
        public List<int>? IndustryOptionIds      { get; set; }
    }
}
