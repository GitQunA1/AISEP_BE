using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Deal
{
    public class RespondDealRequestDtoValidator : AbstractValidator<RespondDealRequestDto>
    {
        public RespondDealRequestDtoValidator()
        {
            RuleFor(x => x.IsAccepted)
                .NotNull().WithMessage("IsAccepted is required.");

            RuleFor(x => x.Reason)
                .MaximumLength(2000).WithMessage("Reason must not exceed 2000 characters.");

            When(x => x.IsAccepted.HasValue && !x.IsAccepted.Value, () =>
            {
                RuleFor(x => x.Reason)
                    .NotEmpty().WithMessage("Reason is required when deal is rejected.");
            });
        }
    }
}
