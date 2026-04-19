using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Auth
{
    public class ResendConfirmationRequestValidator : AbstractValidator<ResendConfirmationRequest>
    {
        public ResendConfirmationRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email format is invalid.")
                .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");
        }
    }
}
