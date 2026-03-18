using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Project
{
    public class RejectProjectRequestValidator : AbstractValidator<RejectProjectRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s]*$";

        public RejectProjectRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required when rejecting a project.")
                .MaximumLength(2000).WithMessage("Reason must not exceed 2000 characters.")
                .Matches(TextPattern).WithMessage("Reason contains invalid characters.");
        }
    }
}
