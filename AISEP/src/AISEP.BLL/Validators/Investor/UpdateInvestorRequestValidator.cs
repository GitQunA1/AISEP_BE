using AISEP.BLL.DTOs.Requests;
using FluentValidation;
using Nethereum.Util;

namespace AISEP.BLL.Validators.Investor
{
    public class UpdateInvestorRequestValidator : AbstractValidator<UpdateInvestorRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s.,!?'-]*$";

        public UpdateInvestorRequestValidator()
        {
            RuleFor(x => x)
                .Must(HasAtLeastOneField)
                .WithMessage("At least one field must be provided for update.");

            RuleFor(x => x.OrganizationName)
                .NotEmpty().WithMessage("Organization name must not be empty when provided.")
                .MaximumLength(255).WithMessage("Organization name must not exceed 255 characters.")
                .Matches(TextPattern).WithMessage("Organization name contains invalid characters.")
                .When(x => x.OrganizationName is not null);

            RuleFor(x => x.InvestmentTaste)
                .NotEmpty().WithMessage("Investment taste must not be empty when provided.")
                .MaximumLength(1000).WithMessage("Investment taste must not exceed 1000 characters.")
                .Matches(TextPattern).WithMessage("Investment taste contains invalid characters.")
                .When(x => x.InvestmentTaste is not null);

            RuleFor(x => x.InvestmentAmount)
                .GreaterThan(0).WithMessage("Investment amount must be greater than 0.")
                .When(x => x.InvestmentAmount.HasValue);

            RuleFor(x => x.InvestmentDate)
                .LessThanOrEqualTo(_ => DateTime.UtcNow.Date)
                .WithMessage("Investment date cannot be in the future.")
                .When(x => x.InvestmentDate.HasValue);

            RuleFor(x => x.InvestmentRegion)
                .NotEmpty().WithMessage("Investment region must not be empty when provided.")
                .MaximumLength(255).WithMessage("Investment region must not exceed 255 characters.")
                .Matches(TextPattern).WithMessage("Investment region contains invalid characters.")
                .When(x => x.InvestmentRegion is not null);

            RuleFor(x => x.WalletAddress)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Wallet address must not be empty when provided.")
                .MaximumLength(255).WithMessage("Wallet address must not exceed 255 characters.")
                .Must(BeValidEthereumWalletAddress).WithMessage("Wallet address must be a valid Ethereum address with a correct EIP-55 checksum.")
                .When(x => x.WalletAddress is not null);

            RuleFor(x => x.PreviousInvestments)
                .NotEmpty().WithMessage("Previous investments must not be empty when provided.")
                .MaximumLength(1000).WithMessage("Previous investments must not exceed 1000 characters.")
                .Matches(TextPattern).WithMessage("Previous investments contains invalid characters.")
                .When(x => x.PreviousInvestments is not null);

            RuleFor(x => x.RiskTolerance)
                .IsInEnum().WithMessage("Risk tolerance is not valid.")
                .When(x => x.RiskTolerance.HasValue);

            RuleFor(x => x.FocusIndustry)
                .IsInEnum().WithMessage("Focus industry is not valid.")
                .When(x => x.FocusIndustry.HasValue);

            RuleFor(x => x.PreferredStage)
                .IsInEnum().WithMessage("Preferred stage is not valid.")
                .When(x => x.PreferredStage.HasValue);
        }

        private static bool HasAtLeastOneField(UpdateInvestorRequest request)
        {
            return request.OrganizationName is not null
                || request.InvestmentTaste is not null
                || request.WalletAddress is not null
                || request.InvestmentAmount.HasValue
                || request.InvestmentDate.HasValue
                || request.RiskTolerance.HasValue
                || request.InvestmentRegion is not null
                || request.FocusIndustry.HasValue
                || request.PreferredStage.HasValue
                || request.PreviousInvestments is not null;
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
