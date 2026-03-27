using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.ConsultingReport
{
    public class CreateConsultingReportRequestValidator : AbstractValidator<CreateConsultingReportRequest>
    {
        public CreateConsultingReportRequestValidator()
        {
            RuleFor(x => x.BookingId)
                .GreaterThan(0).WithMessage("BookingId must be a positive number.");

            RuleFor(x => x.MeetingTitle)
                .NotEmpty().WithMessage("MeetingTitle is required.")
                .MaximumLength(255).WithMessage("MeetingTitle must not exceed 255 characters.");

            RuleFor(x => x.Location)
                .MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.Location))
                .WithMessage("Location must not exceed 255 characters.");

            RuleFor(x => x.MeetingTime)
                .NotEmpty().WithMessage("MeetingTime is required.");
        }
    }
}
