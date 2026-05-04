using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;

namespace AISEP.BLL.Services.ScorecardConfigs
{
    public class ScorecardConfigService : IScorecardConfigService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ScorecardConfigService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ScorecardWeightConfigResponse> GetDefaultConfigAsync()
        {
            var config = await _unitOfWork.ScorecardWeightConfigs.GetByIdAsync(1)
                ?? await _unitOfWork.ScorecardWeightConfigs.GetDefaultAsync()
                ?? throw new KeyNotFoundException("Scorecard weight config not found.");

            return MapResponse(config);
        }

        public async Task<ScorecardWeightConfigResponse> UpdateConfigAsync(int id, UpdateScorecardWeightRequest request)
        {
            var config = await _unitOfWork.ScorecardWeightConfigs.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Scorecard weight config not found.");

            config.TeamWeight = request.TeamWeight;
            config.MarketWeight = request.MarketWeight;
            config.ProductWeight = request.ProductWeight;
            config.CompetitionWeight = request.CompetitionWeight;
            config.TractionWeight = request.TractionWeight;
            config.InvestmentNeedWeight = request.InvestmentNeedWeight;
            config.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ScorecardWeightConfigs.Update(config);
            await _unitOfWork.SaveChangesAsync();

            return MapResponse(config);
        }

        private static ScorecardWeightConfigResponse MapResponse(ScorecardWeightConfig config)
        {
            return new ScorecardWeightConfigResponse
            {
                Id = config.Id,
                ConfigName = config.ConfigName,
                TeamWeight = config.TeamWeight,
                MarketWeight = config.MarketWeight,
                ProductWeight = config.ProductWeight,
                CompetitionWeight = config.CompetitionWeight,
                TractionWeight = config.TractionWeight,
                InvestmentNeedWeight = config.InvestmentNeedWeight,
                CreatedAt = config.CreatedAt,
                UpdatedAt = config.UpdatedAt
            };
        }
    }
}
