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

            RuleFor(x => x.AdvisorAvailabilitySlotIds)
                .NotEmpty().WithMessage("At least one slot must be selected.");

            RuleFor(x => x.AdvisorAvailabilitySlotIds)
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage("Selected slots must be unique.");

            RuleFor(x => x.Note)
                .MaximumLength(1000).WithMessage("Note must not exceed 1000 characters.");
        }
    }
}
