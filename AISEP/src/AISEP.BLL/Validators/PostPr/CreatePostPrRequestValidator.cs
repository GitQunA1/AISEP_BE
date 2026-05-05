using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.PostPr
{
    public class CreatePostPrRequestValidator : AbstractValidator<CreatePostPrRequest>
    {
        public CreatePostPrRequestValidator()
        {
            RuleFor(x => x.DealId)
                .GreaterThan(0).WithMessage("DealId phải là số dương.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title là bắt buộc.")
                .MaximumLength(255).WithMessage("Title không được vượt quá 255 ký tự.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content là bắt buộc.")
                .MaximumLength(10000).WithMessage("Content không được vượt quá 10000 ký tự.");
        }
    }
}
