using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Booking
{
    public class RejectBookingRequestValidator : AbstractValidator<RejectBookingRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s.,;:!?&()%'""-]*$";

        public RejectBookingRequestValidator()
        {
            RuleFor(x => x.Reason)
                .MaximumLength(500)
                .WithMessage("Lý do từ chối không được vượt quá 500 ký tự.")
                .Matches(TextPattern).WithMessage("Lý do từ chối chứa ký tự không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.Reason));
        }
    }
}
