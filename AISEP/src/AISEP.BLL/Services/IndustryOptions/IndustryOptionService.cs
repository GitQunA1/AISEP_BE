using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.IndustryOptions
{
    public class IndustryOptionService : IIndustryOptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;

        public IndustryOptionService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
        }

        // Trả danh sách ngành động có hỗ trợ filter, sort và phân trang bằng Sieve.
        public async Task<PagedResult<IndustryOptionResponse>> GetAllAsync(SieveModel model, bool includeInactive = false)
        {
            var query = includeInactive
                ? _unitOfWork.IndustryOptions.GetAllQuery()
                : _unitOfWork.IndustryOptions.GetActiveQuery();

            query = query.AsNoTracking();

            return await PaginationHelper.PaginateAsync(
                query,
                model,
                _sieveProcessor,
                MapResponse);
        }

        // Tạo option ngành mới để dùng chung cho startup, project, investor và advisor.
        public async Task<IndustryOptionResponse> CreateAsync(CreateIndustryOptionRequest request)
        {
            var value = NormalizeValue(request.Value);
            await EnsureValueIsUniqueAsync(value);

            var entity = new IndustryOption
            {
                Value = value,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.IndustryOptions.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return MapResponse(entity);
        }

        // Cập nhật option ngành hiện có theo id.
        public async Task<IndustryOptionResponse> UpdateAsync(int id, UpdateIndustryOptionRequest request)
        {
            var entity = await _unitOfWork.IndustryOptions.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Industry option not found.");

            var value = NormalizeValue(request.Value);
            await EnsureValueIsUniqueAsync(value, id);

            entity.Value = value;
            entity.IsActive = request.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.IndustryOptions.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return MapResponse(entity);
        }

        private async Task EnsureValueIsUniqueAsync(string value, int? currentId = null)
        {
            var exists = await _unitOfWork.IndustryOptions.GetAllQuery()
                .AnyAsync(x => x.Value.ToLower() == value.ToLower() && (!currentId.HasValue || x.Id != currentId.Value));

            if (exists)
            {
                throw new InvalidOperationException("Industry option value already exists.");
            }
        }

        private static string NormalizeValue(string value)
        {
            var normalized = value?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new InvalidOperationException("Industry value is required.");
            }

            return normalized;
        }

        private static IndustryOptionResponse MapResponse(IndustryOption entity)
        {
            return new IndustryOptionResponse
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
