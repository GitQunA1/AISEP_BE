using Sieve.Attributes;

namespace AISEP.DAL.Entities
{
    public class FormValidationRule
    {
        [Sieve(CanFilter = true, CanSort = true)]
        public int Id { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public string FormKey { get; set; } = string.Empty;

        [Sieve(CanFilter = true, CanSort = true)]
        public string FieldKey { get; set; } = string.Empty;

        [Sieve(CanFilter = true, CanSort = true)]
        public bool IsRequired { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public int? MinLength { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public int? MaxLength { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public string? CustomRegexPattern { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public decimal? MinValue { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public decimal? MaxValue { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public string? AllowedFileTypesJson { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public long? MaxFileSizeBytes { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public DateTime CreatedAt { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public DateTime UpdatedAt { get; set; }
    }
}
