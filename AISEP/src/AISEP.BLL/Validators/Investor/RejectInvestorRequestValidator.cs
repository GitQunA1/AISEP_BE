using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Investor
{
    public class RejectInvestorRequestValidator : AbstractValidator<RejectInvestorRequest>
    {
        public RejectInvestorRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotNull().WithMessage("Lý do không được để null.")
                //.Must(reason => !string.IsNullOrWhiteSpace(reason)).WithMessage("Lý do là bắt buộc.")
                .Matches(@"^[\p{L}\p{N}\s.,!?'\-]*$").WithMessage("Lý do chứa ký tự không hợp lệ.")
                .MaximumLength(2000).WithMessage("Lý do không được vượt quá 2000 ký tự.");
        }
    }
}
