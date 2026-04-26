using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Advisor
{
    public class UpdateAdvisorRequestValidator : AbstractValidator<UpdateAdvisorRequest>
    {
        public UpdateAdvisorRequestValidator()
        {
            RuleFor(x => x)
                .Must(HasAtLeastOneField)
                .WithMessage("Cần cung cấp ít nhất một trường để cập nhật.");

            // Rule field-level đã chuyển sang dynamic validation qua bảng form_validation_rules.
            // File này chỉ giữ lại rule cấu trúc: update phải có ít nhất một field.
        }

        private static bool HasAtLeastOneField(UpdateAdvisorRequest request)
        {
            return request.Bio is not null
                || request.Expertise is not null
                || request.IndustryOptionIds is not null
                || request.PreviousExperience is not null
                || request.LanguagesSpoken is not null
                || request.Location is not null
                || request.HourlyRate.HasValue
                || request.ProfileImageFile is not null
                || request.CertificationFile is not null;
        }
    }
}
