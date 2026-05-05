using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Deal
{
    public class StaffReviewDealRequestDtoValidator : AbstractValidator<StaffReviewDealRequestDto>
    {
        public StaffReviewDealRequestDtoValidator()
        {
            RuleFor(x => x.IsApproved)
                .NotNull().WithMessage("IsApproved là bắt buộc.");

            RuleFor(x => x.Reason)
                .MaximumLength(2000).WithMessage("Reason không được vượt quá 2000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Reason));
        }
    }
}
