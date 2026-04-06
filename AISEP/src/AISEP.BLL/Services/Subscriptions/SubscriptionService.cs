using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.Subscriptions
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISieveProcessor _sieveProcessor;

        public SubscriptionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ISieveProcessor sieveProcessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _sieveProcessor = sieveProcessor;
        }

        public async Task<SubscriptionResponseDto?> GetMySubscriptionAsync(int userId)
        {
            var subscription = await _unitOfWork.Subscriptions.GetQuery()
                .Include(s => s.Package)
                .Include(s => s.User)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            return subscription is null ? null : _mapper.Map<SubscriptionResponseDto>(subscription);
        }

        public async Task<PagedResult<SubscriptionResponseDto>> GetAllSubscriptionsAsync(SieveModel sieveModel)
        {
            sieveModel ??= new SieveModel();
            if (!sieveModel.Page.HasValue || sieveModel.Page.Value <= 0)
            {
                sieveModel.Page = 1;
            }

            if (!sieveModel.PageSize.HasValue || sieveModel.PageSize.Value <= 0)
            {
                sieveModel.PageSize = 10;
            }

            var query = _unitOfWork.Subscriptions.GetQuery()
                .Include(s => s.Package)
                .Include(s => s.User)
                .OrderByDescending(s => s.EndDate)
                .AsNoTracking();

            var totalCount = await _sieveProcessor
                .Apply(sieveModel, query, applyPagination: false, applySorting: false)
                .CountAsync();

            var items = await _sieveProcessor
                .Apply(sieveModel, query)
                .ToListAsync();

            return new PagedResult<SubscriptionResponseDto>
            {
                Page = sieveModel.Page.Value,
                PageSize = sieveModel.PageSize.Value,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)sieveModel.PageSize.Value),
                Items = items.Select(s => _mapper.Map<SubscriptionResponseDto>(s))
            };
        }
    }
}
