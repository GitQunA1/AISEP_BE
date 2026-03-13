using AISEP.BLL.DTOs.Requests;
using AISEP.DAL.Enums;
using FluentValidation;

namespace AISEP.BLL.Validators.Investor
{
    public class CreateInvestorRequestValidator : AbstractValidator<CreateInvestorRequest>
    {
        public CreateInvestorRequestValidator()
        {
            RuleFor(x => x.OrganizationName)
                .NotEmpty().WithMessage("Organization name is required.")
                .MaximumLength(255).WithMessage("Organization name must not exceed 255 characters.")
                .When(x => x.OrganizationName is not null);

            RuleFor(x => x.InvestmentAmount)
                .NotEmpty().WithMessage("Investment amount is required.")
                .GreaterThan(0).WithMessage("Investment amount must be greater than 0.")
                .When(x => x.InvestmentAmount is not null && x.InvestmentAmount != 0);

            RuleFor(x => x.InvestmentRegion)
                .NotEmpty().WithMessage("Investment region is required.")
                .MaximumLength(255).WithMessage("Investment region must not exceed 255 characters.")
                .When(x => x.InvestmentRegion is not null);

            RuleFor(x => x.WalletAddress)
                .NotEmpty().WithMessage("Wallet address is required.")
                .MaximumLength(255).WithMessage("Wallet address must not exceed 255 characters.")
                .When(x => x.WalletAddress is not null);

            RuleFor(x => x.PreviousInvestments)
                .NotEmpty().WithMessage("Previous investments are required.")
                .MaximumLength(1000).WithMessage("Previous investments must not exceed 1000 characters.")
                .When(x => x.PreviousInvestments is not null);

            RuleFor(x => x.RiskTolerance)
                .NotEmpty().WithMessage("Risk tolerance is required.")
                .IsInEnum().WithMessage("Risk tolerance is not valid. Allowed: Low, Medium, High.")
                .When(x => x.RiskTolerance.HasValue);

            RuleFor(x => x.FocusIndustry)
                .NotEmpty().WithMessage("Focus industry is required.")
                .IsInEnum().WithMessage("Focus industry is not valid.")
                .When(x => x.FocusIndustry.HasValue);

            RuleFor(x => x.PreferredStage)
                .NotEmpty().WithMessage("Preferred stage is required.")
                .IsInEnum().WithMessage("Preferred stage is not valid. Allowed: Idea, MVP, Growth, Scale.")
                .When(x => x.PreferredStage.HasValue);
        }
    }
}
