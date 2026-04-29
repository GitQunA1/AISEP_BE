using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Deal
{
    public class VerifyDealRequestDtoValidator : AbstractValidator<VerifyDealRequestDto>
    {
        public VerifyDealRequestDtoValidator()
        {
            RuleFor(x => x.IsConfirmed)
                .NotNull().WithMessage("IsConfirmed is required.");

            RuleFor(x => x.Reason)
                .MaximumLength(2000).WithMessage("Reason must not exceed 2000 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Reason));
        }
    }
}
