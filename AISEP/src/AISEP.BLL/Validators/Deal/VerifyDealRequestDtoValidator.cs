using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Deal
{
    public class VerifyDealRequestDtoValidator : AbstractValidator<VerifyDealRequestDto>
    {
        public VerifyDealRequestDtoValidator()
        {
            RuleFor(x => x.IsConfirmed)
                .NotNull().WithMessage("IsConfirmed là bắt buộc.");

            RuleFor(x => x.Reason)
                .MaximumLength(2000).WithMessage("Reason không được vượt quá 2000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Reason));
        }
    }
}
