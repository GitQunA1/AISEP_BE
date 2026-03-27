using System.Text.Json.Serialization;

namespace AISEP.BLL.DTOs.Requests
{
    public class NftMetadataDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;

        [JsonPropertyName("attributes")]
        public List<NftAttributeDto> Attributes { get; set; } = new();
    }
}
