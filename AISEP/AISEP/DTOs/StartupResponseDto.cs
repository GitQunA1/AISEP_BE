using AISEP.Models.Enums;

namespace AISEP.DTOs
{
    public class StartupResponseDto
    {
        public Guid Id { get; set; }
        public string? CompanyName { get; set; }
        public string? LogoUrl { get; set; }
        public string? Founder { get; set; }
        public string? CountryCity { get; set; }
        public string? Website { get; set; }
        public string? Industry { get; set; }
        public DevelopmentStage? DevelopmentStage { get; set; }
        public string? ProblemStatement { get; set; }
        public string? SolutionDescription { get; set; }
        public decimal? MarketSize { get; set; }
        public decimal? Revenue { get; set; }
        public int FollowerCount { get; set; }
    }
}
