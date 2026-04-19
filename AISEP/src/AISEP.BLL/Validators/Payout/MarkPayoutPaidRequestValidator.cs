using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Payout
{
    public class MarkPayoutPaidRequestValidator : AbstractValidator<MarkPayoutPaidRequest>
    {
        public MarkPayoutPaidRequestValidator()
        {
            RuleFor(x => x.Note)
                .MaximumLength(1000).WithMessage("Note must not exceed 1000 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Note));
        }
    }
}
