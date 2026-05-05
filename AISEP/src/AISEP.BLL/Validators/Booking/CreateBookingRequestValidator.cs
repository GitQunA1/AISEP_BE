using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Booking
{
    public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
    {
        public CreateBookingRequestValidator()
        {
            RuleFor(x => x.AdvisorId)
                .NotEmpty().WithMessage("AdvisorId là bắt buộc.")
                .GreaterThan(0).WithMessage("AdvisorId phải là số dương.");

            RuleFor(x => x.ProjectId)
                .NotEmpty().WithMessage("ProjectId là bắt buộc.")
                .GreaterThan(0).WithMessage("ProjectId phải là số dương.");

            RuleFor(x => x.OldBookingId)
                .GreaterThan(0)
                .When(x => x.OldBookingId.HasValue)
                .WithMessage("OldBookingId phải là số dương.");

            RuleFor(x => x.AdvisorAvailabilitySlotIds)
                .NotEmpty().WithMessage("Cần chọn ít nhất một khung giờ.");

            RuleFor(x => x.AdvisorAvailabilitySlotIds)
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage("Các khung giờ được chọn không được trùng nhau.");

            RuleFor(x => x.Note)
                .MaximumLength(1000).WithMessage("Ghi chú không được vượt quá 1000 ký tự.");
        }
    }
}
