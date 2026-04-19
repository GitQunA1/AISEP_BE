using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.UserReport
{
    public class ResolveUserReportRequestValidator : AbstractValidator<ResolveUserReportRequest>
    {
        public ResolveUserReportRequestValidator()
        {
            RuleFor(x => x.ResolutionNote)
                .MaximumLength(1000)
                .WithMessage("Resolution note must not exceed 1000 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.ResolutionNote));
        }
    }
}
