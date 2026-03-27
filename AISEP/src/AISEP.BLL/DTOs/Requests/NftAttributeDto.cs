using System.Text.Json.Serialization;

namespace AISEP.BLL.DTOs.Requests
{
    public class NftAttributeDto
    {
        [JsonPropertyName("trait_type")]
        public string TraitType { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public object Value { get; set; } = string.Empty;
    }
}
