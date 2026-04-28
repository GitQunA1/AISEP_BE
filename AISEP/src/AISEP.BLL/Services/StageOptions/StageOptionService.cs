using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.StageOptions
{
    public class StageOptionService : IStageOptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;

        public StageOptionService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
        }

        public async Task<PagedResult<StageOptionResponse>> GetAllAsync(SieveModel model)
        {
            var query = _unitOfWork.StageOptions.GetAllQuery()
                .AsNoTracking();

            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, MapResponse);
        }

        public async Task<StageOptionResponse> CreateAsync(CreateStageOptionRequest request)
        {
            var value = NormalizeValue(request.Value);
            await EnsureValueIsUniqueAsync(value);

            var entity = new StageOption
            {
                Value = value,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.StageOptions.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return MapResponse(entity);
        }

        public async Task<StageOptionResponse> SetActiveAsync(int id, bool isActive)
        {
            var entity = await _unitOfWork.StageOptions.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Stage option not found.");

            entity.IsActive = isActive;
            entity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.StageOptions.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return MapResponse(entity);
        }

        private async Task EnsureValueIsUniqueAsync(string value, int? currentId = null)
        {
            var exists = await _unitOfWork.StageOptions.GetAllQuery()
                .AnyAsync(x => x.Value.ToLower() == value.ToLower() && (!currentId.HasValue || x.Id != currentId.Value));

            if (exists)
            {
                throw new InvalidOperationException("Stage option value already exists.");
            }
        }

        private static string NormalizeValue(string value)
        {
            var normalized = value?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new InvalidOperationException("Stage value is required.");
            }

            return normalized;
        }

        private static StageOptionResponse MapResponse(StageOption entity)
        {
            return new StageOptionResponse
            {
                Id = entity.Id,
                Value = entity.Value,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}
