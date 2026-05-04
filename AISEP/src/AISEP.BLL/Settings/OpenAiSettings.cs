namespace AISEP.BLL.Settings
{
    public class OpenAiSettings
    {
        public string ApiKey  { get; set; } = string.Empty;
        public string Model   { get; set; } = "gpt-4.1";
        public string BaseUrl { get; set; } = "https://api.openai.com/v1";
        public int MaxOutputTokens { get; set; } = 8192;
        public double Temperature { get; set; } = 0.05;
    }
}
