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
                .WithMessage("At least one field must be provided for update.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title must not be empty when provided.")
                .MaximumLength(255).WithMessage("Title must not exceed 255 characters.")
                .When(x => x.Title is not null);

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content must not be empty when provided.")
                .MaximumLength(10000).WithMessage("Content must not exceed 10000 characters.")
                .When(x => x.Content is not null);
        }

        private static bool HasAtLeastOneField(UpdatePostPrRequest request)
        {
            return request.Title is not null || request.Content is not null;
        }
    }
}
