using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.UserReport
{
    public class CreateUserReportRequestValidator : AbstractValidator<CreateUserReportRequest>
    {
        public CreateUserReportRequestValidator()
        {
            RuleFor(x => x.ReportedUserId)
                .GreaterThan(0).WithMessage("ReportedUserId must be a positive number.");

            RuleFor(x => x.Reason)
                .MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.Reason))
                .WithMessage("Reason must not exceed 1000 characters.");

            RuleFor(x => x.EvidenceUrl)
                .MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.EvidenceUrl))
                .WithMessage("EvidenceUrl must not exceed 255 characters.");
        }
    }
}
