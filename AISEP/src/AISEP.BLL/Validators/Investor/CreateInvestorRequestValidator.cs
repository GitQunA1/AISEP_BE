using AISEP.BLL.DTOs.Requests;
using AISEP.DAL.Enums;
using FluentValidation;
using Nethereum.Util;

namespace AISEP.BLL.Validators.Investor
{
    public class CreateInvestorRequestValidator : AbstractValidator<CreateInvestorRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s.,;:!?&()%'""-]*$";

        public CreateInvestorRequestValidator()
        {
            RuleFor(x => x.OrganizationName)
                .NotEmpty().WithMessage("Organization name is required.")
                .MaximumLength(255).WithMessage("Organization name must not exceed 255 characters.")
                .Matches(TextPattern).WithMessage("Organization name contains invalid characters.");

            RuleFor(x => x.InvestmentTaste)
                .NotEmpty().WithMessage("Investment taste is required.")
                .MaximumLength(1000).WithMessage("Investment taste must not exceed 1000 characters.")
                .Matches(TextPattern).WithMessage("Investment taste contains invalid characters.");

            RuleFor(x => x.InvestmentAmount)
                .NotEmpty().WithMessage("Investment amount is required.")
                .GreaterThan(0).WithMessage("Investment amount must be greater than 0.");

            RuleFor(x => x.InvestmentRegion)
                .NotEmpty().WithMessage("Investment region is required.")
                .MaximumLength(255).WithMessage("Investment region must not exceed 255 characters.")
                .Matches(TextPattern).WithMessage("Investment region contains invalid characters.");

            RuleFor(x => x.WalletAddress)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Wallet address must not be empty when provided.")
                .MaximumLength(255).WithMessage("Wallet address must not exceed 255 characters.")
                .Must(BeValidEthereumWalletAddress).WithMessage("Wallet address must be a valid Ethereum address with a correct EIP-55 checksum.")
                .When(x => x.WalletAddress is not null);

            RuleFor(x => x.PreviousInvestments)
                .NotEmpty().WithMessage("Previous investments are required.")
                .MaximumLength(1000).WithMessage("Previous investments must not exceed 1000 characters.")
                .Matches(TextPattern).WithMessage("Previous investments contains invalid characters.");

            RuleFor(x => x.RiskTolerance)
                .NotEmpty().WithMessage("Risk tolerance is required.")
                .IsInEnum().WithMessage("Risk tolerance is not valid. Allowed: Low, Medium, High.");

            RuleFor(x => x.FocusIndustry)
                .NotEmpty().WithMessage("Focus industry is required.")
                .IsInEnum().WithMessage("Focus industry is not valid.");

            RuleFor(x => x.PreferredStage)
                .NotEmpty().WithMessage("Preferred stage is required.")
                .IsInEnum().WithMessage("Preferred stage is not valid. Allowed: Idea, MVP, Growth, Scale.");
        }

        private static bool BeValidEthereumWalletAddress(string? walletAddress)
        {
            if (string.IsNullOrWhiteSpace(walletAddress))
            {
                return false;
            }

            var address = walletAddress.Trim();
            var addressUtil = AddressUtil.Current;

            return addressUtil.IsValidEthereumAddressHexFormat(address)
                   && addressUtil.IsChecksumAddress(address);
        }
    }
}
