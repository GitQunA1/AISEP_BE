using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Review
{
    public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
    {
        public CreateReviewRequestValidator()
        {
            RuleFor(x => x.BookingId)
                .NotEmpty().WithMessage("Mã lịch đặt là bắt buộc.")
                .GreaterThan(0).WithMessage("Mã lịch đặt phải là số dương.");

            RuleFor(x => x.Rating)
                .NotEmpty().WithMessage("Đánh giá là bắt buộc.")
                .InclusiveBetween(1, 5).WithMessage("Đánh giá phải nằm trong khoảng từ 1 đến 5.");

            RuleFor(x => x.ReviewContent)
                .NotEmpty().WithMessage("Nội dung đánh giá là bắt buộc.")
                .MaximumLength(2000).WithMessage("Nội dung đánh giá không được vượt quá 2000 ký tự.");
        }
    }
}
