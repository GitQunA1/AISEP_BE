using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Payment
{
    public class UpdatePackageRequestValidator : AbstractValidator<UpdatePackageRequest>
    {
        public UpdatePackageRequestValidator()
        {
            RuleFor(x => x.PackageName)
                .NotEmpty().WithMessage("PackageName là bắt buộc.")
                .MaximumLength(255).WithMessage("PackageName không được vượt quá 255 ký tự.");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description không được vượt quá 2000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price phải lớn hơn 0.");

            RuleFor(x => x.DurationMonths)
                .GreaterThan(0).WithMessage("DurationMonths phải lớn hơn 0.");

            RuleFor(x => x.MaxAiRequests)
                .GreaterThanOrEqualTo(0).WithMessage("MaxAiRequests phải lớn hơn hoặc bằng 0.");

            RuleFor(x => x.MaxProjectViews)
                .GreaterThanOrEqualTo(0).WithMessage("MaxProjectViews phải lớn hơn hoặc bằng 0.");

            RuleFor(x => x.FreeBookingCount)
                .GreaterThanOrEqualTo(0).WithMessage("FreeBookingCount phải lớn hơn hoặc bằng 0.");
        }
    }
}
