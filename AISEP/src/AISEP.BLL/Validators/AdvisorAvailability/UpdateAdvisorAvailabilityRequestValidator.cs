using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.AdvisorAvailability
{
    public class UpdateAdvisorAvailabilityRequestValidator : AbstractValidator<UpdateAdvisorAvailabilityRequest>
    {
        public UpdateAdvisorAvailabilityRequestValidator()
        {
            RuleFor(x => x.SlotDate)
                .NotEmpty().WithMessage("Ngày khả dụng là bắt buộc.")
                .Must(date => date.Date >= DateTime.UtcNow.Date)
                .WithMessage("Ngày khả dụng không được ở quá khứ.");

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("Thời gian bắt đầu là bắt buộc.");

            RuleFor(x => x.EndTime)
                .NotEmpty().WithMessage("Thời gian kết thúc là bắt buộc.")
                .GreaterThan(x => x.StartTime).WithMessage("Thời gian kết thúc phải sau thời gian bắt đầu.");

            RuleFor(x => x)
                .Must(x => x.EndTime == x.StartTime.AddHours(1))
                .WithMessage("Khung giờ khả dụng phải đúng 1 giờ.");

            RuleFor(x => x)
                .Must(x => x.StartTime.Minute == 0 && x.StartTime.Second == 0 && x.EndTime.Minute == 0 && x.EndTime.Second == 0)
                .WithMessage("Khung giờ khả dụng phải bắt đầu và kết thúc đúng giờ.");

            RuleFor(x => x)
                .Must(x => x.SlotDate.Date > DateTime.UtcNow.Date
                    || x.StartTime > TimeOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Khung giờ khả dụng phải ở tương lai.");
        }
    }
}
