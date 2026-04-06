using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Wallet
{
    public class CreateWithdrawRequestDtoValidator : AbstractValidator<CreateWithdrawRequestDto>
    {
        public CreateWithdrawRequestDtoValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0.");

            RuleFor(x => x.BankName)
                .NotEmpty().WithMessage("Bank name is required.")
                .MaximumLength(255);

            RuleFor(x => x.BankAccount)
                .NotEmpty().WithMessage("Bank account is required.")
                .MaximumLength(255);

        }
    }
}
