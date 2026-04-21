using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Auth
{
    public class ResendConfirmationRequestValidator : AbstractValidator<ResendConfirmationRequest>
    {
        public ResendConfirmationRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email là bắt buộc.")
                .EmailAddress().WithMessage("Email không đúng định dạng.")
                .MaximumLength(255).WithMessage("Email không được vượt quá 255 ký tự.");
        }
    }
}
