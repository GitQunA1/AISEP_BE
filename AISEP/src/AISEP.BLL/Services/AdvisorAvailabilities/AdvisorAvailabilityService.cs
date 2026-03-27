using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.AdvisorAvailabilities
{
    public class AdvisorAvailabilityService : IAdvisorAvailabilityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ISieveProcessor _sieveProcessor;

        public AdvisorAvailabilityService(
            IUnitOfWork unitOfWork,
            IUserService userService,
            IMapper mapper,
            ISieveProcessor sieveProcessor)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _mapper = mapper;
            _sieveProcessor = sieveProcessor;
        }

        public async Task<PagedResult<AdvisorAvailabilityResponse>> GetByAdvisorIdAsync(int advisorId, SieveModel model)
        {
            _ = await _unitOfWork.Advisors.GetByIdAsync(advisorId)
                ?? throw new KeyNotFoundException("Advisor not found.");

            var query = _unitOfWork.AdvisorAvailabilities.GetQuery()
                .Where(x => x.AdvisorId == advisorId)
                .Where(x => x.SlotDate.Date > DateTime.UtcNow.Date
                    || (x.SlotDate.Date == DateTime.UtcNow.Date && x.StartTime > TimeOnly.FromDateTime(DateTime.UtcNow)));

            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor,
                x => _mapper.Map<AdvisorAvailabilityResponse>(x));
        }

        public async Task<PagedResult<AdvisorAvailabilityResponse>> GetMyAvailabilitiesAsync(SieveModel model)
        {
            var advisor = await GetCurrentAdvisorAsync();
            var query = _unitOfWork.AdvisorAvailabilities.GetQuery()
                .Where(x => x.AdvisorId == advisor.AdvisorId)
                .Where(x => x.SlotDate.Date > DateTime.UtcNow.Date
                    || (x.SlotDate.Date == DateTime.UtcNow.Date && x.StartTime > TimeOnly.FromDateTime(DateTime.UtcNow)));

            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor,
                x => _mapper.Map<AdvisorAvailabilityResponse>(x));
        }

        public async Task<AdvisorAvailabilityResponse> CreateMyAvailabilityAsync(CreateAdvisorAvailabilityRequest request)
        {
            var advisor = await GetCurrentAdvisorAsync();
            var exists = await _unitOfWork.AdvisorAvailabilities
                .ExistsAsync(advisor.AdvisorId, request.SlotDate, request.StartTime, request.EndTime);
            if (exists)
                throw new InvalidOperationException("Availability slot already exists.");

            var availability = _mapper.Map<AdvisorAvailability>(request);
            availability.AdvisorId = advisor.AdvisorId;
            availability.SlotDate = request.SlotDate.Date;
            availability.Status = AdvisorAvailabilityStatus.Available;
            availability.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.AdvisorAvailabilities.AddAsync(availability);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AdvisorAvailabilityResponse>(availability);
        }

        public async Task<AdvisorAvailabilityResponse> UpdateMyAvailabilityAsync(int availabilityId, UpdateAdvisorAvailabilityRequest request)
        {
            var advisor = await GetCurrentAdvisorAsync();
            var availability = await _unitOfWork.AdvisorAvailabilities.GetByIdAsync(availabilityId)
                ?? throw new KeyNotFoundException("Availability slot not found.");

            if (availability.AdvisorId != advisor.AdvisorId)
                throw new ForbiddenAccessException("You do not have permission to update this availability slot.");

            if (availability.Status == AdvisorAvailabilityStatus.Booked)
                throw new InvalidOperationException("Booked availability slot cannot be updated.");

            var exists = await _unitOfWork.AdvisorAvailabilities
                .ExistsAsync(advisor.AdvisorId, request.SlotDate, request.StartTime, request.EndTime, availabilityId);
            if (exists)
                throw new InvalidOperationException("Availability slot already exists.");

            availability.SlotDate = request.SlotDate.Date;
            availability.StartTime = request.StartTime;
            availability.EndTime = request.EndTime;
            availability.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.AdvisorAvailabilities.Update(availability);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AdvisorAvailabilityResponse>(availability);
        }

        public async Task<bool> DeleteMyAvailabilityAsync(int availabilityId)
        {
            var advisor = await GetCurrentAdvisorAsync();
            var availability = await _unitOfWork.AdvisorAvailabilities.GetByIdAsync(availabilityId);
            if (availability is null || availability.AdvisorId != advisor.AdvisorId)
                return false;

            if (availability.Status == AdvisorAvailabilityStatus.Booked)
                throw new InvalidOperationException("Cannot delete a booked availability slot.");

            await _unitOfWork.AdvisorAvailabilities.DeleteAsync(availabilityId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private async Task<Advisor> GetCurrentAdvisorAsync()
        {
            var userId = _userService.GetUserId();
            var advisor = await _unitOfWork.Advisors.GetByUserIdAsync(userId);
            if (advisor is null)
                throw new KeyNotFoundException("Advisor profile not found.");
            return advisor;
        }
    }
}
