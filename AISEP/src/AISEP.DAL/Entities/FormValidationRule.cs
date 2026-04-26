namespace AISEP.DAL.Entities
{
    public class FormValidationRule
    {
        public int Id { get; set; }
        public string FormKey { get; set; } = string.Empty;
        public string FieldKey { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? CustomRegexPattern { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? AllowedFileTypesJson { get; set; }
        public long? MaxFileSizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
