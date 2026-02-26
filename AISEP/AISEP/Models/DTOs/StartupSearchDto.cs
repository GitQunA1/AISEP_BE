using AISEP.Models.Enums;

namespace AISEP.DTOs
{
    public class StartupSearchDto
    {
        public string? Industry { get; set; }
        public DevelopmentStage? DevelopmentStage { get; set; }
        public string? SearchTerm { get; set; }
    }
}
