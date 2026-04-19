using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Payment
{
    public class UpdatePackageRequestValidator : AbstractValidator<UpdatePackageRequest>
    {
        public UpdatePackageRequestValidator()
        {
            RuleFor(x => x.PackageName)
                .NotEmpty().WithMessage("PackageName is required.")
                .MaximumLength(255).WithMessage("PackageName must not exceed 255 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(x => x.DurationMonths)
                .GreaterThan(0).WithMessage("DurationMonths must be greater than 0.");

            RuleFor(x => x.MaxAiRequests)
                .GreaterThanOrEqualTo(0).WithMessage("MaxAiRequests must be greater than or equal to 0.");

            RuleFor(x => x.MaxProjectViews)
                .GreaterThanOrEqualTo(0).WithMessage("MaxProjectViews must be greater than or equal to 0.");

            RuleFor(x => x.FreeBookingCount)
                .GreaterThanOrEqualTo(0).WithMessage("FreeBookingCount must be greater than or equal to 0.");
        }
    }
}
