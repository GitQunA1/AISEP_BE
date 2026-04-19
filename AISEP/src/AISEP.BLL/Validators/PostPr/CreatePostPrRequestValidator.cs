using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.PostPr
{
    public class CreatePostPrRequestValidator : AbstractValidator<CreatePostPrRequest>
    {
        public CreatePostPrRequestValidator()
        {
            RuleFor(x => x.DealId)
                .GreaterThan(0).WithMessage("DealId must be a positive number.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(255).WithMessage("Title must not exceed 255 characters.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content is required.")
                .MaximumLength(10000).WithMessage("Content must not exceed 10000 characters.");
        }
    }
}
