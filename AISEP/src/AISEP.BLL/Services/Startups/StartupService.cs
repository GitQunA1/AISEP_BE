using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Storage;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.Startups
{
    public class StartupService : IStartupService
    {
        private readonly IUnitOfWork     _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper         _mapper;
        private readonly IUserService    _userService;
        private readonly IStorageService _storage;

        public StartupService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor, IMapper mapper, IUserService userService, IStorageService storage)
        {
            _unitOfWork     = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper         = mapper;
            _userService    = userService;
            _storage        = storage;
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
            if (startup is null)
                throw new KeyNotFoundException("Startup not found.");
            return _mapper.Map<StartupResponse>(startup);
        }

        public async Task<StartupResponse?> GetMyProfileAsync()
        {
            var userId = _userService.GetUserId();
            var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId);
            if (startup is null)
                throw new KeyNotFoundException("Startup profile not found.");
            return _mapper.Map<StartupResponse>(startup);
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

        public async Task<StartupResponse> CreateStartupAsync(CreateStartupRequest dto)
        { 
            var userId = _userService.GetUserId();
            var existing = await _unitOfWork.Startups.GetByUserIdAsync(userId);
            if (existing is not null)
                throw new InvalidOperationException("Startup profile already exists for this account.");

            var logoUrl = await UploadIfPresent(dto.LogoFile, "startup-logos");
            var businessLicenseUrl = await UploadIfPresent(dto.BusinessLicenseFile, "startup-licenses");

            var startup = new Startup
            {
                UserId             = userId,
                CompanyName        = dto.CompanyName,
                Founder            = dto.Founder,
                Email              = dto.Email,
                PhoneNumber        = dto.PhoneNumber,
                CountryCity        = dto.CountryCity,
                Website            = dto.Website,
                Industry           = dto.Industry,
                LogoUrl            = logoUrl,
                BusinessLicenseUrl = businessLicenseUrl,
                ApprovalStatus     = ApprovalStatus.Pending,
                CreatedAt          = DateTime.UtcNow
               
            };

            await _unitOfWork.Startups.AddAsync(startup);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<StartupResponse>(startup);
        }

        public async Task<StartupResponse> UpdateStartupAsync(int id, UpdateStartupRequest dto)
        {
            var userId = _userService.GetUserId(); 
            var startup = await _unitOfWork.Startups.GetByIdAsync(id);
            if (startup is null)
                throw new KeyNotFoundException("Startup profile not found.");

            if (startup.UserId != userId)
                throw new ForbiddenAccessException("You do not have permission to update this startup.");

            startup.CompanyName = string.IsNullOrWhiteSpace(dto.CompanyName) ? startup.CompanyName : dto.CompanyName;
            startup.Founder     = string.IsNullOrWhiteSpace(dto.Founder)     ? startup.Founder     : dto.Founder;
            startup.Email       = string.IsNullOrWhiteSpace(dto.Email)       ? startup.Email       : dto.Email;
            startup.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? startup.PhoneNumber : dto.PhoneNumber;
            startup.CountryCity = string.IsNullOrWhiteSpace(dto.CountryCity) ? startup.CountryCity : dto.CountryCity;
            startup.Website     = (!string.IsNullOrWhiteSpace(dto.Website) && Uri.TryCreate(dto.Website, UriKind.Absolute, out _))
                                   ? dto.Website
                                   : startup.Website;
            startup.Industry    = dto.Industry ?? startup.Industry;

            if (dto.LogoFile is not null)
                startup.LogoUrl = await _storage.UploadFileAsync(dto.LogoFile, "startup-logos");

            if (dto.BusinessLicenseFile is not null)
                startup.BusinessLicenseUrl = await _storage.UploadFileAsync(dto.BusinessLicenseFile, "startup-licenses");

            if (startup.ApprovalStatus == ApprovalStatus.Rejected)
            {
                startup.ApprovalStatus = ApprovalStatus.Pending;
            }

            _unitOfWork.Startups.Update(startup);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<StartupResponse>(startup);
        }

        public async Task ApproveStartupAsync(int startupId)
        {
            var userId = _userService.GetUserId();
            var startup = await _unitOfWork.Startups.GetByIdAsync(startupId);
            if (startup is null)
                throw new KeyNotFoundException("Startup not found.");
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

        public async Task RejectStartupAsync(int startupId, RejectStartupRequest dto)
        {
            var userId = _userService.GetUserId();
            var startup = await _unitOfWork.Startups.GetByIdAsync(startupId);
            if (startup is null)
                throw new KeyNotFoundException("Startup not found.");
            if (startup.ApprovalStatus == ApprovalStatus.Rejected)
                throw new InvalidOperationException("Startup is already rejected.");
            if (startup.ApprovalStatus != ApprovalStatus.Pending)
                throw new InvalidOperationException($"Only Pending startups can be rejected. Current status: {startup.ApprovalStatus}.");

            startup.ApprovalStatus  = ApprovalStatus.Rejected;
            startup.RejectedAt      = DateTime.UtcNow;
            startup.RejectionReason = dto.Reason?.Trim();
            startup.RejectedById    = userId; 

            _unitOfWork.Startups.Update(startup);
            await _unitOfWork.SaveChangesAsync();
        }



        private async Task<string?> UploadIfPresent(IFormFile? file, string folder)
            => file is not null ? await _storage.UploadFileAsync(file, folder) : null;

       
    }
        }
