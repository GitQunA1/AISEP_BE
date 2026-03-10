using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Project
{
    public class ApproveProjectRequestValidator : AbstractValidator<ApproveProjectRequest>
    {
        public ApproveProjectRequestValidator()
        {
            RuleFor(x => x.Note)
                .MaximumLength(1000).WithMessage("Note must not exceed 1000 characters.")
                .When(x => x.Note is not null);
        }
    }
}
