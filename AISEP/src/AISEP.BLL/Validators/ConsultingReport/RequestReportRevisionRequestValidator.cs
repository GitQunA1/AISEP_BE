using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.ConsultingReport
{
    public class RequestReportRevisionRequestValidator : AbstractValidator<RequestReportRevisionRequest>
    {
        public RequestReportRevisionRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required.")
                .MinimumLength(3).WithMessage("Reason must be at least 3 characters.")
                .MaximumLength(2000).WithMessage("Reason must not exceed 2000 characters.");
        }
    }
}
