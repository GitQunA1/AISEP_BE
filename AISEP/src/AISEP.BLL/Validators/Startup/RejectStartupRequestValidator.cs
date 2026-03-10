using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Startup
{
    public class RejectStartupRequestValidator : AbstractValidator<RejectStartupRequest>
    {
        public RejectStartupRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required when rejecting a startup.")
                .MaximumLength(2000).WithMessage("Reason must not exceed 2000 characters.");
        }
    }
}
