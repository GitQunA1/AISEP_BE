using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Reject
{
    public class RejectRequestValidator : AbstractValidator<RejectRequest>
    {
        public RejectRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotNull().WithMessage("Reason must not be null.")
                //.Must(reason => !string.IsNullOrWhiteSpace(reason)).WithMessage("Reason is required.")
                .Matches(@"^[\p{L}\p{N}\s]+$").WithMessage("Reason must not contain special characters.")
                .MaximumLength(2000).WithMessage("Reason must not exceed 2000 characters.");
        }
    }
}
