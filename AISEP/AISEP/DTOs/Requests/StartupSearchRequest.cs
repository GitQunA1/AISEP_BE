using AISEP.Models.Enums;

namespace AISEP.DTOs.Requests
{
    public class StartupSearchRequest
    {
        public string? Industry { get; set; }
        public DevelopmentStage? DevelopmentStage { get; set; }
        public string? SearchTerm { get; set; }
    }
}
