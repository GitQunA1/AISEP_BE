using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.ScorecardConfigs
{
    public class UpdateScorecardWeightRequestValidator : AbstractValidator<UpdateScorecardWeightRequest>
    {
        public UpdateScorecardWeightRequestValidator()
        {
            RuleFor(x => x.TeamWeight)
                .GreaterThanOrEqualTo(0).WithMessage("TeamWeight không được âm.");

            RuleFor(x => x.MarketWeight)
                .GreaterThanOrEqualTo(0).WithMessage("MarketWeight không được âm.");

            RuleFor(x => x.ProductWeight)
                .GreaterThanOrEqualTo(0).WithMessage("ProductWeight không được âm.");

            RuleFor(x => x.CompetitionWeight)
                .GreaterThanOrEqualTo(0).WithMessage("CompetitionWeight không được âm.");

            RuleFor(x => x.TractionWeight)
                .GreaterThanOrEqualTo(0).WithMessage("TractionWeight không được âm.");

            RuleFor(x => x.InvestmentNeedWeight)
                .GreaterThanOrEqualTo(0).WithMessage("InvestmentNeedWeight không được âm.");

            RuleFor(x => x)
                .Must(HaveExactlyOneHundredTotalWeight)
                .WithMessage("Tổng các trọng số phải bằng đúng 100%");
        }

        private static bool HaveExactlyOneHundredTotalWeight(UpdateScorecardWeightRequest request)
        {
            var total = request.TeamWeight
                + request.MarketWeight
                + request.ProductWeight
                + request.CompetitionWeight
                + request.TractionWeight
                + request.InvestmentNeedWeight;

            return total == 100.0m;
        }
    }
}
