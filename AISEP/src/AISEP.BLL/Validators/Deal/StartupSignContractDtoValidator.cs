using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Deal
{
    public class StartupSignContractDtoValidator : AbstractValidator<StartupSignContractDto>
    {
        public StartupSignContractDtoValidator()
        {
            RuleFor(x => x.SignatureBase64)
                .NotEmpty().WithMessage("SignatureBase64 is required.");
        }
    }
}
