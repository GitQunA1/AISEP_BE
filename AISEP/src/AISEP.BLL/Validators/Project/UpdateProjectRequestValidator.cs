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
        }

        private static bool HasAtLeastOneField(UpdateProjectRequest request)
        {
            return request.ProjectName is not null
                || request.ProjectImageFile is not null
                || request.ShortDescription is not null
                || request.StageOptionId.HasValue
                || request.ProblemStatement is not null
                || request.SolutionDescription is not null
                || request.TargetCustomers is not null
                || request.UniqueValueProposition is not null
                || request.BusinessModel is not null
                || request.Competitors is not null
                || request.IndustryOptionId.HasValue
                || request.TeamSize.HasValue
                || request.TeamExperience.HasValue
                || request.HasTechnicalCofounder.HasValue
                || request.TargetMarketSize.HasValue
                || request.MarketGrowth.HasValue
                || request.ProductReadiness.HasValue
                || request.IPProtection.HasValue
                || request.BarrierToEntry.HasValue
                || request.CurrentTraction.HasValue
                || request.RunwayMonths.HasValue;
        }
    }
}
