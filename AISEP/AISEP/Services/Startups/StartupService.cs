using AISEP.Common;
using AISEP.Data;
using AISEP.DTOs;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;
     

        public StartupService(IStartupRepository repository, ISieveProcessor sieveProcessor, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
           
        }

       

        public async Task<PagedResultDto<StartupResponseDto>> SearchStartupsAsync(SieveModel model, string? industry = null, DevelopmentStage? stage = null)
        {
            var query = _unitOfWork.Startups.SearchStartupsQuery(industry, stage);
            return await ApplySieveAndPaginateAsync(query, model);
        }

        public async Task<StartupResponseDto?> GetStartupByIdAsync(int id)
        {
            var startup = await _unitOfWork.Startups.GetByIdAsync(id);
            return startup is null ? null : _mapper.Map<StartupResponseDto>(startup);
        }

        public async Task<PagedResultDto<StartupResponseDto>> GetAllStartupsAsync(SieveModel model)
        {
            var query = _unitOfWork.Startups.GetStartupQuery();
            return await ApplySieveAndPaginateAsync(query, model);
        }

        public async Task<PagedResultDto<StartupResponseDto>> GetStartupsByStatusAsync(SieveModel model, ApprovalStatus? status = null)
        {
            var query = _unitOfWork.Startups.GetByStatusQuery(status);
            return await ApplySieveAndPaginateAsync(query, model);
        }

      

        public async Task<StartupResponseDto> CreateStartupAsync(int userId, CreateStartupDto dto)
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

            return _mapper.Map<StartupResponseDto>(startup);
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

        //public async Task<StartupResponseDto?> GetMyProfileAsync(int userId)
        //{
        //    var startup = await _repository.GetByUserIdAsync(userId);
        //    return startup is null ? null : _mapper.Map<StartupResponseDto>(startup);
        //}

      

        //public async Task<PagedResultDto<StartupResponseDto>> GetPendingStartupsAsync(SieveModel model)
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

    

        private async Task<PagedResultDto<StartupResponseDto>> ApplySieveAndPaginateAsync(
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

            return new PagedResultDto<StartupResponseDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Items = items.Select(s => _mapper.Map<StartupResponseDto>(s))
            };
        }
    }
}
