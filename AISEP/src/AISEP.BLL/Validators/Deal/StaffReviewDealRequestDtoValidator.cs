using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Deal
{
    public class StaffReviewDealRequestDtoValidator : AbstractValidator<StaffReviewDealRequestDto>
    {
        public StaffReviewDealRequestDtoValidator()
        {
            RuleFor(x => x.IsApproved)
                .NotNull().WithMessage("IsApproved is required.");

            RuleFor(x => x.Reason)
                .MaximumLength(2000).WithMessage("Reason must not exceed 2000 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Reason));
        }
    }
}
