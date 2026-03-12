using AISEP.BLL.DTOs.Requests;
using AISEP.DAL.Enums;
using FluentValidation;

namespace AISEP.BLL.Validators.Investor
{
    public class UpdateInvestorRequestValidator : AbstractValidator<UpdateInvestorRequest>
    {
        public UpdateInvestorRequestValidator()
        {
            RuleFor(x => x.OrganizationName)
                .MaximumLength(255).WithMessage("Organization name must not exceed 255 characters.")
                .When(x => x.OrganizationName is not null);

            RuleFor(x => x.InvestmentAmount)
                .GreaterThan(0).WithMessage("Investment amount must be greater than 0.")
                .When(x => x.InvestmentAmount.HasValue);

            RuleFor(x => x.InvestmentRegion)
                .MaximumLength(255).WithMessage("Investment region must not exceed 255 characters.")
                .When(x => x.InvestmentRegion is not null);

            RuleFor(x => x.WalletAddress)
                .MaximumLength(255).WithMessage("Wallet address must not exceed 255 characters.")
                .When(x => x.WalletAddress is not null);

            RuleFor(x => x.PreviousInvestments)
                .MaximumLength(1000).WithMessage("Previous investments must not exceed 1000 characters.")
                .When(x => x.PreviousInvestments is not null);

            RuleFor(x => x.RiskTolerance)
                .IsInEnum().WithMessage("Risk tolerance is not valid. Allowed: Low, Medium, High.")
                .When(x => x.RiskTolerance.HasValue);

            RuleFor(x => x.PreferredStage)
                .IsInEnum().WithMessage("Preferred stage is not valid. Allowed: Idea, MVP, Growth, Scale.")
                .When(x => x.PreferredStage.HasValue);
        }
    }
}
