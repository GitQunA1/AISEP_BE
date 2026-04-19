using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Payout
{
    public class RejectPayoutRequestValidator : AbstractValidator<RejectPayoutRequest>
    {
        public RejectPayoutRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reject reason is required.")
                .MaximumLength(1000).WithMessage("Reject reason must not exceed 1000 characters.");

            RuleFor(x => x.Note)
                .MaximumLength(1000).WithMessage("Note must not exceed 1000 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Note));
        }
    }
}
