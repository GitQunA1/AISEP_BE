using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Booking
{
    public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
    {
        public CreateBookingRequestValidator()
        {
            RuleFor(x => x.AdvisorId)
                .GreaterThan(0).WithMessage("AdvisorId must be a positive number.");

            RuleFor(x => x.StartTime)
                .GreaterThan(DateTime.UtcNow).WithMessage("StartTime must be in the future.");

            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime).WithMessage("EndTime must be after StartTime.");

            RuleFor(x => x.EndTime)
                .Must((req, end) => (end - req.StartTime).TotalMinutes >= 30)
                .WithMessage("Booking duration must be at least 30 minutes.");
        }
    }
}
