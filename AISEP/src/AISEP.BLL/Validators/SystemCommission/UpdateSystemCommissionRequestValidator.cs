using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.SystemCommission
{
    public class UpdateSystemCommissionRequestValidator : AbstractValidator<UpdateSystemCommissionRequest>
    {
        public UpdateSystemCommissionRequestValidator()
        {
            RuleFor(x => x.Percent)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(100);

            RuleFor(x => x.Reason)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrWhiteSpace(x.Reason));
        }
    }
}
