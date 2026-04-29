using AISEP.BLL.DTOs.Requests;
using FluentValidation;
using System.Text.RegularExpressions;

namespace AISEP.BLL.Validators.FormValidationRules
{
    public class UpsertFormValidationRuleRequestValidator : AbstractValidator<UpsertFormValidationRuleRequest>
    {
        public UpsertFormValidationRuleRequestValidator()
        {
            RuleFor(x => x.MinLength)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MinLength.HasValue);

            RuleFor(x => x.MaxLength)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MaxLength.HasValue);

            RuleFor(x => x)
                .Must(x => !x.MinLength.HasValue || !x.MaxLength.HasValue || x.MinLength <= x.MaxLength)
                .WithMessage("MinLength cannot be greater than MaxLength.");

            RuleFor(x => x)
                .Must(x => !x.MinValue.HasValue || !x.MaxValue.HasValue || x.MinValue <= x.MaxValue)
                .WithMessage("MinValue cannot be greater than MaxValue.");

            RuleFor(x => x.CustomRegexPattern)
                .Must(BeValidRegex)
                .When(x => !string.IsNullOrWhiteSpace(x.CustomRegexPattern))
                .WithMessage("CustomRegexPattern must be a valid regular expression.");

            RuleForEach(x => x.AllowedFileTypes)
                .NotEmpty()
                .When(x => x.AllowedFileTypes is not null);
        }

        private static bool BeValidRegex(string? pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                return true;
            }

            try
            {
                _ = new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(1));
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
