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
                .NotEmpty().WithMessage("Tên tổ chức là bắt buộc.")
                .MaximumLength(255).WithMessage("Tên tổ chức không được vượt quá 255 ký tự.")
                .Matches(TextPattern).WithMessage("Tên tổ chức chứa ký tự không hợp lệ.");

            RuleFor(x => x.InvestmentTaste)
                .NotEmpty().WithMessage("Khẩu vị đầu tư là bắt buộc.")
                .MaximumLength(1000).WithMessage("Khẩu vị đầu tư không được vượt quá 1000 ký tự.")
                .Matches(TextPattern).WithMessage("Khẩu vị đầu tư chứa ký tự không hợp lệ.");

            RuleFor(x => x.InvestmentAmount)
                .NotEmpty().WithMessage("Số tiền đầu tư là bắt buộc.")
                .GreaterThan(0).WithMessage("Số tiền đầu tư phải lớn hơn 0.");

            RuleFor(x => x.InvestmentRegion)
                .NotEmpty().WithMessage("Khu vực đầu tư là bắt buộc.")
                .MaximumLength(255).WithMessage("Khu vực đầu tư không được vượt quá 255 ký tự.")
                .Matches(TextPattern).WithMessage("Khu vực đầu tư chứa ký tự không hợp lệ.");

            RuleFor(x => x.WalletAddress)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Địa chỉ ví không được để trống khi được cung cấp.")
                .MaximumLength(255).WithMessage("Địa chỉ ví không được vượt quá 255 ký tự.")
                .Must(BeValidEthereumWalletAddress).WithMessage("Địa chỉ ví phải là địa chỉ Ethereum hợp lệ và đúng checksum EIP-55.")
                .When(x => x.WalletAddress is not null);

            RuleFor(x => x.PreviousInvestments)
                .NotEmpty().WithMessage("Kinh nghiệm đầu tư trước đây là bắt buộc.")
                .MaximumLength(1000).WithMessage("Kinh nghiệm đầu tư trước đây không được vượt quá 1000 ký tự.")
                .Matches(TextPattern).WithMessage("Kinh nghiệm đầu tư trước đây chứa ký tự không hợp lệ.");

            RuleFor(x => x.RiskTolerance)
                .NotEmpty().WithMessage("Mức chịu rủi ro là bắt buộc.")
                .IsInEnum().WithMessage("Mức chịu rủi ro không hợp lệ. Giá trị hợp lệ: Low, Medium, High.");

            RuleFor(x => x.FocusIndustry)
                .NotEmpty().WithMessage("Lĩnh vực tập trung là bắt buộc.")
                .IsInEnum().WithMessage("Lĩnh vực tập trung không hợp lệ.");

            RuleFor(x => x.PreferredStage)
                .NotEmpty().WithMessage("Giai đoạn ưu tiên là bắt buộc.")
                .IsInEnum().WithMessage("Giai đoạn ưu tiên không hợp lệ. Giá trị hợp lệ: Idea, MVP, Growth, Scale.");
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
