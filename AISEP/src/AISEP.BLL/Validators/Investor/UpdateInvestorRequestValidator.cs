using AISEP.BLL.DTOs.Requests;
using FluentValidation;
using Nethereum.Util;

namespace AISEP.BLL.Validators.Investor
{
    public class UpdateInvestorRequestValidator : AbstractValidator<UpdateInvestorRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s.,;:!?&()%'""-]*$";

        public UpdateInvestorRequestValidator()
        {
            RuleFor(x => x)
                .Must(HasAtLeastOneField)
                .WithMessage("Cần cung cấp ít nhất một trường để cập nhật.");

            RuleFor(x => x.OrganizationName)
                .NotEmpty().WithMessage("Tên tổ chức không được để trống khi được cung cấp.")
                .MaximumLength(255).WithMessage("Tên tổ chức không được vượt quá 255 ký tự.")
                .Matches(TextPattern).WithMessage("Tên tổ chức chứa ký tự không hợp lệ.")
                .When(x => x.OrganizationName is not null);

            RuleFor(x => x.InvestmentTaste)
                .NotEmpty().WithMessage("Khẩu vị đầu tư không được để trống khi được cung cấp.")
                .MaximumLength(1000).WithMessage("Khẩu vị đầu tư không được vượt quá 1000 ký tự.")
                .Matches(TextPattern).WithMessage("Khẩu vị đầu tư chứa ký tự không hợp lệ.")
                .When(x => x.InvestmentTaste is not null);

            RuleFor(x => x.InvestmentAmount)
                .GreaterThan(0).WithMessage("Số tiền đầu tư phải lớn hơn 0.")
                .When(x => x.InvestmentAmount.HasValue);

            RuleFor(x => x.InvestmentDate)
                .LessThanOrEqualTo(_ => DateTime.UtcNow.Date)
                .WithMessage("Ngày đầu tư không được ở tương lai.")
                .When(x => x.InvestmentDate.HasValue);

            RuleFor(x => x.InvestmentRegion)
                .NotEmpty().WithMessage("Khu vực đầu tư không được để trống khi được cung cấp.")
                .MaximumLength(255).WithMessage("Khu vực đầu tư không được vượt quá 255 ký tự.")
                .Matches(TextPattern).WithMessage("Khu vực đầu tư chứa ký tự không hợp lệ.")
                .When(x => x.InvestmentRegion is not null);

            RuleFor(x => x.WalletAddress)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Địa chỉ ví không được để trống khi được cung cấp.")
                .MaximumLength(255).WithMessage("Địa chỉ ví không được vượt quá 255 ký tự.")
                .Must(BeValidEthereumWalletAddress).WithMessage("Địa chỉ ví phải là địa chỉ Ethereum hợp lệ và đúng checksum EIP-55.")
                .When(x => x.WalletAddress is not null);

            RuleFor(x => x.PreviousInvestments)
                .NotEmpty().WithMessage("Kinh nghiệm đầu tư trước đây không được để trống khi được cung cấp.")
                .MaximumLength(1000).WithMessage("Kinh nghiệm đầu tư trước đây không được vượt quá 1000 ký tự.")
                .Matches(TextPattern).WithMessage("Kinh nghiệm đầu tư trước đây chứa ký tự không hợp lệ.")
                .When(x => x.PreviousInvestments is not null);

            RuleFor(x => x.RiskTolerance)
                .IsInEnum().WithMessage("Mức chịu rủi ro không hợp lệ.")
                .When(x => x.RiskTolerance.HasValue);

            RuleFor(x => x.FocusIndustry)
                .IsInEnum().WithMessage("Lĩnh vực tập trung không hợp lệ.")
                .When(x => x.FocusIndustry.HasValue);

            RuleFor(x => x.PreferredStage)
                .IsInEnum().WithMessage("Giai đoạn ưu tiên không hợp lệ.")
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
