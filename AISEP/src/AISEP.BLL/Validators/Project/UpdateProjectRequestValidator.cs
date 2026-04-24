using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Project
{
    public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
    {
        public UpdateProjectRequestValidator()
        {
            RuleFor(x => x)
                .Must(HasAtLeastOneField)
                .WithMessage("Cần cung cấp ít nhất một trường để cập nhật.");

            // Rule field-level đã chuyển sang dynamic validation qua bảng form_validation_rules.
            // File này chỉ giữ lại rule cấu trúc: update phải có ít nhất một field.
        }

        private static bool HasAtLeastOneField(UpdateProjectRequest request)
        {
            return request.ProjectName is not null
                || request.ProjectImageFile is not null
                || request.ShortDescription is not null
                || request.DevelopmentStage.HasValue
                || request.ProblemStatement is not null
                || request.SolutionDescription is not null
                || request.TargetCustomers is not null
                || request.UniqueValueProposition is not null
                || request.MarketSize.HasValue
                || request.BusinessModel is not null
                || request.Revenue.HasValue
                || request.Competitors is not null
                || request.TeamMembers is not null
                || request.KeySkills is not null
                || request.TeamExperience is not null
                || request.Industry.HasValue;
        }
    }
}
