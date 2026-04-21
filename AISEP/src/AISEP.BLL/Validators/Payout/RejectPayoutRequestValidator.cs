using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Payout
{
    public class RejectPayoutRequestValidator : AbstractValidator<RejectPayoutRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s.,;:!?&()%'""-]*$";

        public RejectPayoutRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Lý do từ chối là bắt buộc.")
                .MaximumLength(1000).WithMessage("Lý do từ chối không được vượt quá 1000 ký tự.")
                .Matches(TextPattern).WithMessage("Lý do từ chối chứa ký tự không hợp lệ.");

            RuleFor(x => x.Note)
                .MaximumLength(1000).WithMessage("Ghi chú không được vượt quá 1000 ký tự.")
                .Matches(TextPattern).WithMessage("Ghi chú chứa ký tự không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.Note));
        }
    }
}
