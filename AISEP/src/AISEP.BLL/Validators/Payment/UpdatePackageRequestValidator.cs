using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Payment
{
    public class UpdatePackageRequestValidator : AbstractValidator<UpdatePackageRequest>
    {
        public UpdatePackageRequestValidator()
        {
            RuleFor(x => x.PackageName)
                .NotEmpty().WithMessage("Tên gói là bắt buộc.")
                .MaximumLength(255).WithMessage("Tên gói không được vượt quá 255 ký tự.");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Mô tả không được vượt quá 2000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Giá phải lớn hơn 0.");

            RuleFor(x => x.DurationMonths)
                .GreaterThan(0).WithMessage("Số tháng hiệu lực phải lớn hơn 0.");

            RuleFor(x => x.MaxAiRequests)
                .GreaterThanOrEqualTo(0).WithMessage("Số lượt AI tối đa phải lớn hơn hoặc bằng 0.");

            RuleFor(x => x.MaxProjectViews)
                .GreaterThanOrEqualTo(0).WithMessage("Số lượt xem dự án tối đa phải lớn hơn hoặc bằng 0.");

            RuleFor(x => x.FreeBookingCount)
                .GreaterThanOrEqualTo(0).WithMessage("Số lượt đặt lịch miễn phí phải lớn hơn hoặc bằng 0.");
        }
    }
}
