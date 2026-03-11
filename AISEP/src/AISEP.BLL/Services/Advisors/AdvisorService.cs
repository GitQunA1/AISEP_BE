using AISEP.BLL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AutoMapper;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.Advisors
{
    public class AdvisorService : IAdvisorService
    {
        private readonly IUnitOfWork    _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper        _mapper;

        public AdvisorService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor, IMapper mapper)
        {
            _unitOfWork     = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper         = mapper;
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

        public async Task<AdvisorResponse?> CreateAsync(int userId, AdvisorRequest dto)
        {
            var existing = await _unitOfWork.Advisors.GetByUserIdAsync(userId);
            if (existing is not null)
                return null;

            var advisor = _mapper.Map<Advisor>(dto);
            advisor.UserId = userId;

            await _unitOfWork.Advisors.AddAsync(advisor);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.Advisors.GetByIdAsync(advisor.AdvisorId);
            return _mapper.Map<AdvisorResponse>(created!);
        }

        public async Task<AdvisorResponse?> UpdateAsync(int userId, AdvisorRequest dto)
        {
            var advisor = await _unitOfWork.Advisors.GetByUserIdAsync(userId);
            if (advisor is null)
                return null;

            _mapper.Map(dto, advisor);
            _unitOfWork.Advisors.Update(advisor);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AdvisorResponse>(advisor);
        }

        public async Task<bool> DeleteAsync(int advisorId)
        {
            var advisor = await _unitOfWork.Advisors.GetByIdAsync(advisorId);
            if (advisor is null)
                return false;

            await _unitOfWork.Advisors.DeleteAsync(advisorId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
