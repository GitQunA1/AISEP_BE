using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.AdvisorAvailability
{
    public class CreateAdvisorAvailabilityRequestValidator : AbstractValidator<CreateAdvisorAvailabilityRequest>
    {
        public CreateAdvisorAvailabilityRequestValidator()
        {
            RuleFor(x => x.SlotDate)
                .NotEmpty().WithMessage("SlotDate is required.")
                .Must(date => date.Date >= DateTime.UtcNow.Date)
                .WithMessage("SlotDate cannot be in the past.");

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("StartTime is required.");

            RuleFor(x => x.EndTime)
                .NotEmpty().WithMessage("EndTime is required.")
                .GreaterThan(x => x.StartTime).WithMessage("EndTime must be after StartTime.");

            RuleFor(x => x)
                .Must(x =>
                {
                    var duration = x.EndTime.ToTimeSpan() - x.StartTime.ToTimeSpan();
                    return duration > TimeSpan.Zero
                           && duration.TotalHours >= 1
                           && duration.Ticks % TimeSpan.TicksPerHour == 0;
                })
                .WithMessage("Availability range must be at least 1 hour and aligned to full-hour blocks.");

            RuleFor(x => x)
                .Must(x => x.StartTime.Minute == 0 && x.StartTime.Second == 0 && x.EndTime.Minute == 0 && x.EndTime.Second == 0)
                .WithMessage("Availability slot must align to full hours.");

            RuleFor(x => x)
                .Must(x => x.SlotDate.Date > DateTime.UtcNow.Date
                    || x.StartTime > TimeOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Availability slot must be in the future.");
        }
    }
}
