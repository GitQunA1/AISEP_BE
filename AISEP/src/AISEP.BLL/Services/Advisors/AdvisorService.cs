using AISEP.BLL.Helpers;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Services.Storage;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AutoMapper;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.Advisors
{
    public class AdvisorService : IAdvisorService
    {
        private readonly IUnitOfWork     _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper         _mapper;
        private readonly IStorageService _storage;

        public AdvisorService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor, IMapper mapper, IStorageService storage)
        {
            _unitOfWork     = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper         = mapper;
            _storage        = storage;
        }

        public async Task<PagedResult<AdvisorResponse>> GetAllAsync(SieveModel model)
        {
            var query = _unitOfWork.Advisors.GetAllQuery();
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor,
                a => _mapper.Map<AdvisorResponse>(a));
        }

        public async Task<AdvisorResponse?> GetByIdAsync(int advisorId)
        {
            var advisor = await _unitOfWork.Advisors.GetByIdAsync(advisorId);
            return advisor is null ? null : _mapper.Map<AdvisorResponse>(advisor);
        }

        public async Task<AdvisorResponse?> GetMyProfileAsync(int userId)
        {
            var advisor = await _unitOfWork.Advisors.GetByUserIdAsync(userId);
            return advisor is null ? null : _mapper.Map<AdvisorResponse>(advisor);
        }

        public async Task<AdvisorResponse?> CreateAsync(int userId, CreateAdvisorRequest dto)
        {
            var existing = await _unitOfWork.Advisors.GetByUserIdAsync(userId);
            if (existing is not null) return null;

            var advisor        = _mapper.Map<Advisor>(dto);
            advisor.UserId     = userId;
            advisor.ProfileImage   = await UploadIfPresent(dto.ProfileImageFile,  "advisor-profiles");
            advisor.Certifications = await UploadIfPresent(dto.CertificationFile, "advisor-certifications");

            await _unitOfWork.Advisors.AddAsync(advisor);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.Advisors.GetByIdAsync(advisor.AdvisorId);
            return _mapper.Map<AdvisorResponse>(created!);
        }

        public async Task<AdvisorResponse?> UpdateAsync(int userId, UpdateAdvisorRequest dto)
        {
            var advisor = await _unitOfWork.Advisors.GetByUserIdAsync(userId);
            if (advisor is null) return null;

            advisor.Bio                = string.IsNullOrWhiteSpace(dto.Bio)                ? advisor.Bio                : dto.Bio;
            advisor.Expertise          = string.IsNullOrWhiteSpace(dto.Expertise)          ? advisor.Expertise          : dto.Expertise;
            advisor.PreviousExperience = string.IsNullOrWhiteSpace(dto.PreviousExperience) ? advisor.PreviousExperience : dto.PreviousExperience;
            advisor.LanguagesSpoken    = string.IsNullOrWhiteSpace(dto.LanguagesSpoken)    ? advisor.LanguagesSpoken    : dto.LanguagesSpoken;
            advisor.Location           = string.IsNullOrWhiteSpace(dto.Location)           ? advisor.Location           : dto.Location;
            advisor.HourlyRate         = (dto.HourlyRate > 0) ? dto.HourlyRate : advisor.HourlyRate;

            if (dto.ProfileImageFile is not null)
                advisor.ProfileImage = await _storage.UploadFileAsync(dto.ProfileImageFile, "advisor-profiles");

            if (dto.CertificationFile is not null)
                advisor.Certifications = await _storage.UploadFileAsync(dto.CertificationFile, "advisor-certifications");

            _unitOfWork.Advisors.Update(advisor);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AdvisorResponse>(advisor);
        }

        public async Task<bool> DeleteAsync(int advisorId)
        {
            var advisor = await _unitOfWork.Advisors.GetByIdAsync(advisorId);
            if (advisor is null) return false;

            await _unitOfWork.Advisors.DeleteAsync(advisorId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private async Task<string?> UploadIfPresent(IFormFile? file, string folder)
            => file is not null ? await _storage.UploadFileAsync(file, folder) : null;
    }
}
