using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.AdvisorAvailability
{
    public class CreateAdvisorAvailabilityRequestValidator : AbstractValidator<CreateAdvisorAvailabilityRequest>
    {
        public CreateAdvisorAvailabilityRequestValidator()
        {
            RuleFor(x => x.SlotDate)
                .NotEmpty().WithMessage("SlotDate là bắt buộc.")
                .Must(date => date.Date >= DateTime.UtcNow.Date)
                .WithMessage("SlotDate không được ở trong quá khứ.");

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("StartTime là bắt buộc.");

            RuleFor(x => x.EndTime)
                .NotEmpty().WithMessage("EndTime là bắt buộc.")
                .GreaterThan(x => x.StartTime).WithMessage("EndTime phải sau StartTime.");

            RuleFor(x => x)
                .Must(x =>
                {
                    var duration = x.EndTime.ToTimeSpan() - x.StartTime.ToTimeSpan();
                    return duration > TimeSpan.Zero
                           && duration.TotalHours >= 1
                           && duration.Ticks % TimeSpan.TicksPerHour == 0;
                })
                .WithMessage("Khoảng thời gian khả dụng phải ít nhất 1 giờ và căn theo khung tròn giờ.");

            RuleFor(x => x)
                .Must(x => x.StartTime.Minute == 0 && x.StartTime.Second == 0 && x.EndTime.Minute == 0 && x.EndTime.Second == 0)
                .WithMessage("Khung giờ khả dụng phải tròn giờ.");

            RuleFor(x => x)
                .Must(x => x.SlotDate.Date > DateTime.UtcNow.Date
                    || x.StartTime > TimeOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Khung giờ khả dụng phải ở trong tương lai.");
        }
    }
}
