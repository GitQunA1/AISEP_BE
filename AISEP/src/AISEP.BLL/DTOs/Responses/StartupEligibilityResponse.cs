using System.Text.Json.Serialization;

namespace AISEP.BLL.DTOs.Responses
{
    public class StartupEligibilityResponse
    {
        [JsonPropertyName("is_eligible_startup")]
        public bool IsEligibleStartup { get; set; }

        [JsonPropertyName("eligibility_reason")]
        public string EligibilityReason { get; set; } = string.Empty;
    }
}