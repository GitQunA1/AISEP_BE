using AISEP.BLL.Common;
using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.BLL.Services.Users;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.Startups
{
    public class StartupService : IStartupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public StartupService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor, IMapper mapper, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<PagedResult<StartupResponse>> SearchStartupsAsync(SieveModel model, string? industry = null, string? stage = null)
        {
            DevelopmentStage? parsedStage = Enum.TryParse<DevelopmentStage>(stage, ignoreCase: true, out var stageResult)
                ? stageResult : null;
            var query = _unitOfWork.Startups.SearchStartupsQuery(industry, parsedStage);
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, s => _mapper.Map<StartupResponse>(s));
        }

        public async Task<StartupResponse?> GetStartupByIdAsync(int id)
        {
            var startup = await _unitOfWork.Startups.GetByIdAsync(id);
            return startup is null ? null : _mapper.Map<StartupResponse>(startup);
        }

        public async Task<PagedResult<StartupResponse>> GetAllStartupsAsync(SieveModel model)
        {
            var query = _unitOfWork.Startups.GetStartupQuery();
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, s => _mapper.Map<StartupResponse>(s));
        }

        public async Task<PagedResult<StartupResponse>> GetStartupsByStatusAsync(SieveModel model, string? status = null)
        {
            ApprovalStatus? parsedStatus = Enum.TryParse<ApprovalStatus>(status, ignoreCase: true, out var statusResult)
                ? statusResult : null;
            var query = _unitOfWork.Startups.GetByStatusQuery(parsedStatus);
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, s => _mapper.Map<StartupResponse>(s));
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

        public async Task<StartupResponse> UpdateStartupAsync(UpdateStartupRequest dto)
        {
            var userId  = _userService.GetUserId();
            var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId);
            if (startup is null)
                throw new KeyNotFoundException("Startup profile not found for this account.");

            startup.CompanyName        = dto.CompanyName;
            startup.LogoUrl            = dto.LogoUrl;
            startup.Founder            = dto.Founder;
            startup.ContactInfo        = dto.ContactInfo;
            startup.CountryCity        = dto.CountryCity;
            startup.Website            = dto.Website;
            startup.Industry           = dto.Industry;
            startup.BusinessLicenseUrl = dto.BusinessLicenseUrl;

            _unitOfWork.Startups.Update(startup);
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

            if (startup.ApprovalStatus != ApprovalStatus.Pending)
                throw new InvalidOperationException($"Only Pending startups can be approved. Current status: {startup.ApprovalStatus}.");

            startup.ApprovalStatus = ApprovalStatus.Approved;
            startup.ApprovedAt     = DateTime.UtcNow;
            startup.ApprovedById   = userId;

            _unitOfWork.Startups.Update(startup);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RejectStartupAsync(int userId, RejectStartupRequest dto)
        {
            var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId);
            if (startup is null)
                throw new KeyNotFoundException("Startup profile not found.");
            if (startup.ApprovalStatus == ApprovalStatus.Rejected)
                throw new InvalidOperationException("Startup is already rejected.");

            if (startup.ApprovalStatus != ApprovalStatus.Pending)
                throw new InvalidOperationException($"Only Pending startups can be rejected. Current status: {startup.ApprovalStatus}.");

            startup.ApprovalStatus = ApprovalStatus.Rejected;
            startup.RejectedAt = DateTime.UtcNow;
            startup.RejectionReason = dto.Reason?.Trim();
            startup.RejectedById = userId;

            _unitOfWork.Startups.Update(startup);
            await _unitOfWork.SaveChangesAsync();
        }

            }
        }
