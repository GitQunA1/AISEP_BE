using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.AdvisorBankAccount
{
    public class CreateAdvisorBankAccountRequestValidator : AbstractValidator<CreateAdvisorBankAccountRequest>
    {
        public CreateAdvisorBankAccountRequestValidator()
        {
            ApplyCommonRules();
        }

        private void ApplyCommonRules()
        {
            RuleFor(x => x.BankName)
                .NotEmpty().WithMessage("Tên ngân hàng là bắt buộc.")
                .MaximumLength(255).WithMessage("Tên ngân hàng không được vượt quá 255 ký tự.");

            RuleFor(x => x.AccountNumber)
                .NotEmpty().WithMessage("Số tài khoản là bắt buộc.")
                .MaximumLength(100).WithMessage("Số tài khoản không được vượt quá 100 ký tự.");

            RuleFor(x => x.AccountHolderName)
                .NotEmpty().WithMessage("Tên chủ tài khoản là bắt buộc.")
                .MaximumLength(255).WithMessage("Tên chủ tài khoản không được vượt quá 255 ký tự.");
        }
    }
}
