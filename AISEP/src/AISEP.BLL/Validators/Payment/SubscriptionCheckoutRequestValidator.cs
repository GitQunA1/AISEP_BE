using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Payment
{
    public class SubscriptionCheckoutRequestValidator : AbstractValidator<SubscriptionCheckoutRequest>
    {
        public SubscriptionCheckoutRequestValidator()
        {
            RuleFor(x => x.PackageId)
                .GreaterThan(0).WithMessage("PackageId phải lớn hơn 0.");
        }
    }
}
