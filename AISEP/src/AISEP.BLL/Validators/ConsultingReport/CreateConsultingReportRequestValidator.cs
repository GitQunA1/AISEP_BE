using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.ConsultingReport
{
    public class CreateConsultingReportRequestValidator : AbstractValidator<CreateConsultingReportRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s.,;:!?&()%'""-]*$";

        public CreateConsultingReportRequestValidator()
        {
            RuleFor(x => x.BookingId)
                .GreaterThan(0).WithMessage("BookingId phải lớn hơn 0.");

            RuleFor(x => x.MeetingTitle)
                .NotEmpty().WithMessage("Tiêu đề buổi họp là bắt buộc.")
                .MaximumLength(255).WithMessage("Tiêu đề buổi họp không được vượt quá 255 ký tự.")
                .Matches(TextPattern).WithMessage("Tiêu đề buổi họp chứa ký tự không hợp lệ.");

            RuleFor(x => x.Location)
                .MaximumLength(255).WithMessage("Địa điểm không được vượt quá 255 ký tự.")
                .Matches(TextPattern).WithMessage("Địa điểm chứa ký tự không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.Location));

            RuleFor(x => x.MeetingTime)
                .NotEmpty().WithMessage("Thời gian họp là bắt buộc.");

            RuleFor(x => x.MeetingPurpose)
                .MaximumLength(1000).WithMessage("Mục đích buổi họp không được vượt quá 1000 ký tự.")
                .Matches(TextPattern).WithMessage("Mục đích buổi họp chứa ký tự không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.MeetingPurpose));

            RuleFor(x => x.Content)
                .MaximumLength(5000).WithMessage("Nội dung buổi họp không được vượt quá 5000 ký tự.")
                .Matches(TextPattern).WithMessage("Nội dung buổi họp chứa ký tự không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.Content));

            RuleFor(x => x.DecisionsMade)
                .MaximumLength(3000).WithMessage("Kết luận/quyết định không được vượt quá 3000 ký tự.")
                .Matches(TextPattern).WithMessage("Kết luận/quyết định chứa ký tự không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.DecisionsMade));
        }
    }
}
