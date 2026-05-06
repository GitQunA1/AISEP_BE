using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Review
{
    public class UpdateReviewRequestValidator : AbstractValidator<UpdateReviewRequest>
    {
        public UpdateReviewRequestValidator()
        {
            RuleFor(x => x.Rating)
<<<<<<< HEAD
                .NotEmpty().WithMessage("Đánh giá là bắt buộc.")
                .InclusiveBetween(1, 5).WithMessage("Đánh giá phải nằm trong khoảng từ 1 đến 5.");
=======
                .NotEmpty().WithMessage("Rating là bắt buộc.")
                .InclusiveBetween(1, 5).WithMessage("Rating phải nằm trong khoảng 1 đến 5.");
>>>>>>> main

            RuleFor(x => x.ReviewContent)
                .NotEmpty().WithMessage("Nội dung đánh giá là bắt buộc.")
                .MaximumLength(2000).WithMessage("Nội dung đánh giá không được vượt quá 2000 ký tự.");
        }
    }
}
