using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Payout
{
    public class RequestPayoutRetryRequestValidator : AbstractValidator<RequestPayoutRetryRequest>
    {
        public RequestPayoutRetryRequestValidator()
        {
            RuleFor(x => x.ResolutionNote)
                .NotEmpty().WithMessage("Resolution note is required.")
                .MaximumLength(1000).WithMessage("Resolution note must not exceed 1000 characters.");
        }
    }
}
