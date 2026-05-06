using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.PostPr
{
    public class UpdatePostPrRequestValidator : AbstractValidator<UpdatePostPrRequest>
    {
        public UpdatePostPrRequestValidator()
        {
            RuleFor(x => x)
                .Must(HasAtLeastOneField)
                .WithMessage("Cần cung cấp ít nhất một trường để cập nhật.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Tiêu đề không được để trống khi được cung cấp.")
                .MaximumLength(255).WithMessage("Tiêu đề không được vượt quá 255 ký tự.")
                .When(x => x.Title is not null);

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Nội dung không được để trống khi được cung cấp.")
                .MaximumLength(10000).WithMessage("Nội dung không được vượt quá 10000 ký tự.")
                .When(x => x.Content is not null);
        }

        private static bool HasAtLeastOneField(UpdatePostPrRequest request)
        {
            return request.Title is not null || request.Content is not null;
        }
    }
}
