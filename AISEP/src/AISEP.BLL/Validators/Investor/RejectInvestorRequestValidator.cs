using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Investor
{
    public class RejectInvestorRequestValidator : AbstractValidator<RejectInvestorRequest>
    {
        public RejectInvestorRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotNull().WithMessage("Reason must not be null.")
                //.Must(reason => !string.IsNullOrWhiteSpace(reason)).WithMessage("Reason is required.")
                .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$").WithMessage("Reason must not contains invalid characters.")
                .MaximumLength(2000).WithMessage("Reason must not exceed 2000 characters.");
        }
    }
}
