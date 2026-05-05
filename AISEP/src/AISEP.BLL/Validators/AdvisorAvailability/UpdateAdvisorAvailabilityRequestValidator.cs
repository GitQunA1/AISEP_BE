using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.AdvisorAvailability
{
    public class UpdateAdvisorAvailabilityRequestValidator : AbstractValidator<UpdateAdvisorAvailabilityRequest>
    {
        public UpdateAdvisorAvailabilityRequestValidator()
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
                .Must(x => x.EndTime == x.StartTime.AddHours(1))
                .WithMessage("Khung giờ khả dụng phải đúng 1 giờ.");

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
