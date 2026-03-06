using AISEP.Data;
using AISEP.DTOs;
using AISEP.DTOs.Requests;
using AISEP.DTOs.Responses;
using AISEP.Models.Entities;
using AISEP.Models.Enums;
using AISEP.Repositories.Startups;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.Services.Startups
{
    public class StartupService : IStartupService
    {
        private readonly IStartupRepository _repository;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public StartupService(IStartupRepository repository, ISieveProcessor sieveProcessor, IMapper mapper, ApplicationDbContext context)
        {
            _repository = repository;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
            _context = context;
        }

        // ── Public ────────────────────────────────────────────────────────

        public async Task<PagedResult<StartupResponse>> SearchStartupsAsync(SieveModel model, string? industry = null, DevelopmentStage? stage = null)
        {
            var query = _repository.SearchStartupsQuery(industry, stage);
            return await ApplySieveAndPaginateAsync(query, model);
        }

        public async Task<StartupResponse?> GetStartupByIdAsync(int id)
        {
            var startup = await _repository.GetByIdAsync(id);
            return startup is null ? null : _mapper.Map<StartupResponse>(startup);
        }

        public async Task<PagedResult<StartupResponse>> GetAllStartupsAsync(SieveModel model)
        {
            var query = _repository.GetStartupQuery();
            return await ApplySieveAndPaginateAsync(query, model);
        }

        public async Task<PagedResult<StartupResponse>> GetStartupsByStatusAsync(SieveModel model, ApprovalStatus? status = null)
        {
            var query = _repository.GetByStatusQuery(status);
            return await ApplySieveAndPaginateAsync(query, model);
        }

      

        public async Task<StartupResponse> CreateStartupAsync(int userId, CreateStartupRequest dto)
        {
            var existing = await _repository.GetByUserIdAsync(userId);
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

            await _repository.AddAsync(startup);
            await _context.SaveChangesAsync();

            return _mapper.Map<StartupResponse>(startup);
        }

        public async Task ApproveStartupAsync(int userId)
        {
            var startup = await _repository.GetByUserIdAsync(userId);
            if (startup is null)
                throw new KeyNotFoundException("Startup profile not found.");

            if (startup.ApprovalStatus == ApprovalStatus.Approved)
                throw new InvalidOperationException("Startup is already approved.");

            if (startup.ApprovalStatus == ApprovalStatus.Pending)
                throw new InvalidOperationException("Startup is already pending review.");

            startup.ApprovalStatus = ApprovalStatus.Approved;
            _repository.Update(startup);
            await _context.SaveChangesAsync();
        }

        //public async Task<StartupResponseDto?> GetMyProfileAsync(int userId)
        //{
        //    var startup = await _repository.GetByUserIdAsync(userId);
        //    return startup is null ? null : _mapper.Map<StartupResponse>(startup);
        //}

      

        //public async Task<PagedResult<StartupResponse>> GetPendingStartupsAsync(SieveModel model)
        //{
        //    var query = _repository.GetPendingStartupsQuery();
        //    return await ApplySieveAndPaginateAsync(query, model);
        //}

        //public async Task ReviewStartupAsync(int startupId, ReviewStartupDto dto)
        //{
        //    if (dto.Status != ApprovalStatus.Approved && dto.Status != ApprovalStatus.Rejected)
        //        throw new ArgumentException("Status must be Approved or Rejected.");

        //    if (dto.Status == ApprovalStatus.Rejected && string.IsNullOrWhiteSpace(dto.Reason))
        //        throw new ArgumentException("Reason is required when rejecting a startup.");

        //    var startup = await _repository.GetByIdAsync(startupId);
        //    if (startup is null)
        //        throw new KeyNotFoundException("Startup not found.");

        //    if (startup.ApprovalStatus != ApprovalStatus.Pending)
        //        throw new InvalidOperationException($"Startup is not in Pending status. Current status: {startup.ApprovalStatus}.");

        //    startup.ApprovalStatus = dto.Status;
        //    _repository.Update(startup);
        //    await _context.SaveChangesAsync();
        //}

    

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

