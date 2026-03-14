using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Booking
{
    public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
    {
        public CreateBookingRequestValidator()
        {
            RuleFor(x => x.AdvisorId)
                .NotEmpty().WithMessage("AdvisorId is required.")
                .GreaterThan(0).WithMessage("AdvisorId must be a positive number.");

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("StartTime is required.")
                .GreaterThan(DateTime.UtcNow).WithMessage("StartTime must be in the future.");

            RuleFor(x => x.EndTime)
                .NotEmpty().WithMessage("EndTime is required.")
                .GreaterThan(x => x.StartTime).WithMessage("EndTime must be after StartTime.");
                // .Must((req, end) => (end - req.StartTime).TotalMinutes >= 30)
                // .WithMessage("Booking duration must be at least 30 minutes.");
        }
    }
}
