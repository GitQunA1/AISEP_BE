using AISEP.Common;
using AISEP.DTOs.Requests;
using AISEP.DTOs.Responses;
using AISEP.Models.Entities;
using AISEP.Models.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.Services.Startups
{
    public class StartupService : IStartupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;

        public StartupService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
        }

        public async Task<PagedResult<StartupResponse>> SearchStartupsAsync(SieveModel model, string? industry = null, string? stage = null)
        {
            DevelopmentStage? parsedStage = Enum.TryParse<DevelopmentStage>(stage, ignoreCase: true, out var stageResult)
                ? stageResult : null;
            var query = _unitOfWork.Startups.SearchStartupsQuery(industry, parsedStage);
            return await ApplySieveAndPaginateAsync(query, model);
        }

        public async Task<StartupResponse?> GetStartupByIdAsync(int id)
        {
            var startup = await _unitOfWork.Startups.GetByIdAsync(id);
            return startup is null ? null : _mapper.Map<StartupResponse>(startup);
        }

        public async Task<PagedResult<StartupResponse>> GetAllStartupsAsync(SieveModel model)
        {
            var query = _unitOfWork.Startups.GetStartupQuery();
            return await ApplySieveAndPaginateAsync(query, model);
        }

        public async Task<PagedResult<StartupResponse>> GetStartupsByStatusAsync(SieveModel model, string? status = null)
        {
            ApprovalStatus? parsedStatus = Enum.TryParse<ApprovalStatus>(status, ignoreCase: true, out var statusResult)
                ? statusResult : null;
            var query = _unitOfWork.Startups.GetByStatusQuery(parsedStatus);
            return await ApplySieveAndPaginateAsync(query, model);
        }

        public async Task<StartupResponse> CreateStartupAsync(int userId, CreateStartupRequest dto)
        {
            var existing = await _unitOfWork.Startups.GetByUserIdAsync(userId);
            if (existing is not null)
                throw new InvalidOperationException("Startup profile already exists for this account.");

            var startup = new Startup
            {
                UserId = userId,
                CompanyName = dto.CompanyName,
                LogoUrl = dto.LogoUrl,
                Founder = dto.Founder,
                ContactInfo = dto.ContactInfo,
                CountryCity = dto.CountryCity,
                Website = dto.Website,
                Industry = dto.Industry,
                BusinessLicenseUrl = dto.BusinessLicenseUrl,
                ApprovalStatus = ApprovalStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Startups.AddAsync(startup);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<StartupResponse>(startup);
        }

        public async Task ApproveStartupAsync(int userId)
        {
            var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId);
            if (startup is null)
                throw new KeyNotFoundException("Startup profile not found.");

            if (startup.ApprovalStatus == ApprovalStatus.Approved)
                throw new InvalidOperationException("Startup is already approved.");

            if (startup.ApprovalStatus == ApprovalStatus.Pending)
                throw new InvalidOperationException("Startup is already pending review.");

            startup.ApprovalStatus = ApprovalStatus.Approved;
            _unitOfWork.Startups.Update(startup);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<PagedResult<StartupResponse>> ApplySieveAndPaginateAsync(
            IQueryable<Startup> query,
            SieveModel sieveModel)
        {
            var totalCount = await _sieveProcessor
                .Apply(sieveModel, query, applyPagination: false, applySorting: false)
                .CountAsync();

            var items = await _sieveProcessor
                .Apply(sieveModel, query)
                .ToListAsync();

            var page = sieveModel.Page ?? 1;
            var pageSize = sieveModel.PageSize ?? 10;

            return new PagedResult<StartupResponse>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Items = items.Select(s => _mapper.Map<StartupResponse>(s))
            };
        }
    }
}
