using AISEP.DTOs.Requests;
using FluentValidation;

namespace AISEP.Validators.Review
{
    public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
    {
        public CreateReviewRequestValidator()
        {
            RuleFor(x => x.AdvisorId)
                .GreaterThan(0).WithMessage("AdvisorId must be a positive number.");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

            RuleFor(x => x.ReviewContent)
                .MaximumLength(2000).WithMessage("Review content must not exceed 2000 characters.")
                .When(x => x.ReviewContent is not null);
        }
    }
}
