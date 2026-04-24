using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.SystemTerm
{
    public class CreateSystemTermRequestValidator : AbstractValidator<CreateSystemTermRequest>
    {
        public CreateSystemTermRequestValidator()
        {
            RuleFor(x => x.ContentHtml)
                .NotEmpty().WithMessage("Nội dung điều khoản là bắt buộc.");

            RuleFor(x => x.Version)
                .NotEmpty().WithMessage("Phiên bản điều khoản là bắt buộc.")
                .MaximumLength(50).WithMessage("Phiên bản điều khoản không được vượt quá 50 ký tự.");
        }
    }
}
