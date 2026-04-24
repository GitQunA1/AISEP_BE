using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Investor
{
    public class UpdateInvestorRequestValidator : AbstractValidator<UpdateInvestorRequest>
    {
        public UpdateInvestorRequestValidator()
        {
            RuleFor(x => x)
                .Must(HasAtLeastOneField)
                .WithMessage("Cần cung cấp ít nhất một trường để cập nhật.");

            // Rule field-level đã chuyển sang dynamic validation qua bảng form_validation_rules.
            // File này chỉ giữ lại rule cấu trúc: update phải có ít nhất một field.
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
    }
}
