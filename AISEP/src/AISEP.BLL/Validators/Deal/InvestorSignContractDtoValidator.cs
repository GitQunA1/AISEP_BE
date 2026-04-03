using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Deal
{
    public class InvestorSignContractDtoValidator : AbstractValidator<InvestorSignContractDto>
    {
        public InvestorSignContractDtoValidator()
        {
            RuleFor(x => x.FinalAmount)
                .GreaterThan(0).WithMessage("FinalAmount must be greater than 0.");

            RuleFor(x => x.FinalEquityPercentage)
                .GreaterThanOrEqualTo(0).WithMessage("FinalEquityPercentage must be greater than or equal to 0.");

            RuleFor(x => x.AdditionalTerms)
                .MaximumLength(5000).WithMessage("AdditionalTerms must not exceed 5000 characters.");

            RuleFor(x => x.SignatureBase64)
                .NotEmpty().WithMessage("SignatureBase64 is required.");
        }
    }
}
