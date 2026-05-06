using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.PostPr
{
    public class CreatePostPrRequestValidator : AbstractValidator<CreatePostPrRequest>
    {
        public CreatePostPrRequestValidator()
        {
            RuleFor(x => x.DealId)
                .GreaterThan(0).WithMessage("Mã thương vụ phải là số dương.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Tiêu đề là bắt buộc.")
                .MaximumLength(255).WithMessage("Tiêu đề không được vượt quá 255 ký tự.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Nội dung là bắt buộc.")
                .MaximumLength(10000).WithMessage("Nội dung không được vượt quá 10000 ký tự.");
        }
    }
}
