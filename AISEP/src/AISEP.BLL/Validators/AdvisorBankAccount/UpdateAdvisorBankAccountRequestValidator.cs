using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.AdvisorBankAccount
{
    public class UpdateAdvisorBankAccountRequestValidator : AbstractValidator<UpdateAdvisorBankAccountRequest>
    {
        public UpdateAdvisorBankAccountRequestValidator()
        {
            RuleFor(x => x.BankName)
                .NotEmpty().WithMessage("Bank name is required.")
                .MaximumLength(255).WithMessage("Bank name must not exceed 255 characters.");

            RuleFor(x => x.AccountNumber)
                .NotEmpty().WithMessage("Account number is required.")
                .MaximumLength(100).WithMessage("Account number must not exceed 100 characters.");

            RuleFor(x => x.AccountHolderName)
                .NotEmpty().WithMessage("Account holder name is required.")
                .MaximumLength(255).WithMessage("Account holder name must not exceed 255 characters.");
        }
    }
}
