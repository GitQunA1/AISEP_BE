namespace AISEP.BLL.DTOs.Responses
{
    public class FormValidationRuleResponse
    {
        public int Id { get; set; }
        public string FieldKey { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? CustomRegexPattern { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public List<string>? AllowedFileTypes { get; set; }
        public long? MaxFileSizeBytes { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
