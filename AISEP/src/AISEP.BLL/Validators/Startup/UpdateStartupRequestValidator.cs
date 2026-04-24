using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Startup
{
    public class UpdateStartupRequestValidator : AbstractValidator<UpdateStartupRequest>
    {
        public UpdateStartupRequestValidator()
        {
            RuleFor(x => x)
                .Must(HasAtLeastOneField)
                .WithMessage("Cần cung cấp ít nhất một trường để cập nhật.");

            // Rule field-level đã chuyển sang dynamic validation qua bảng form_validation_rules.
            // File này chỉ giữ lại rule cấu trúc: update phải có ít nhất một field.
        }

        private static bool HasAtLeastOneField(UpdateStartupRequest request)
        {
            return request.CompanyName is not null
                || request.Founder is not null
                || request.Email is not null
                || request.PhoneNumber is not null
                || request.CountryCity is not null
                || request.Website is not null
                || request.Industry.HasValue
                || request.LogoFile is not null
                || request.BusinessLicenseFile is not null;
        }
    }
}
