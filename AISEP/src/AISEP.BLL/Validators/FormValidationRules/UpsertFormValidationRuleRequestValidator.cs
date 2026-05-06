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
                .WithMessage("Độ dài tối thiểu không được lớn hơn độ dài tối đa.");

            RuleFor(x => x)
                .Must(x => !x.MinValue.HasValue || !x.MaxValue.HasValue || x.MinValue <= x.MaxValue)
                .WithMessage("Giá trị tối thiểu không được lớn hơn giá trị tối đa.");

            RuleFor(x => x.CustomRegexPattern)
                .Must(BeValidRegex)
                .When(x => !string.IsNullOrWhiteSpace(x.CustomRegexPattern))
                .WithMessage("Mẫu biểu thức chính quy tùy chỉnh phải hợp lệ.");

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
