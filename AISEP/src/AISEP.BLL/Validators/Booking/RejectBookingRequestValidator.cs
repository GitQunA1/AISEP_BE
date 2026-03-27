using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Booking
{
    public class RejectBookingRequestValidator : AbstractValidator<RejectBookingRequest>
    {
        public RejectBookingRequestValidator()
        {
            RuleFor(x => x.Reason)
                .MaximumLength(500)
                .WithMessage("Reject reason must not exceed 500 characters.");
        }
    }
}
