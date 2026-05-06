using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Payout
{
    public class MarkPayoutPaidRequestValidator : AbstractValidator<MarkPayoutPaidRequest>
    {
        public MarkPayoutPaidRequestValidator()
        {
            RuleFor(x => x.Note)
                .MaximumLength(1000).WithMessage("Ghi chú không được vượt quá 1000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Note));
        }
    }
}
