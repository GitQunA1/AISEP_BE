using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Deal
{
    public class StartupRejectContractDtoValidator : AbstractValidator<StartupRejectContractDto>
    {
        public StartupRejectContractDtoValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required when startup rejects a contract.")
                .MaximumLength(2000).WithMessage("Reason must not exceed 2000 characters.");
        }
    }
}
