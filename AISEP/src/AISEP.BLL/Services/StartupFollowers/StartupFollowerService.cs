using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AutoMapper;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.StartupFollowers
{
    public class StartupFollowerService : IStartupFollowerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _currentUserService;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;

        public StartupFollowerService(
            IUnitOfWork unitOfWork,
            IUserService currentUserService,
            ISieveProcessor sieveProcessor,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
        }

        public async Task<bool> FollowStartupAsync(int startupId)
        {
            var userId = _currentUserService.GetUserId();
            var startup = await _unitOfWork.Startups.GetByIdAsync(startupId);
            if (startup is null)
                throw new KeyNotFoundException("Startup not found.");

            var exists = await _unitOfWork.StartupFollowers.IsFollowingAsync(userId, startupId);
            if (exists)
                return false;

            var follow = new StartupFollower
            {
                FollowerId = userId,
                FollowedId = startupId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.StartupFollowers.AddAsync(follow);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnfollowStartupAsync(int startupId)
        {
            var userId = _currentUserService.GetUserId();

            var exists = await _unitOfWork.StartupFollowers.IsFollowingAsync(userId, startupId);
            if (!exists)
                return false;

            await _unitOfWork.StartupFollowers.RemoveAsync(userId, startupId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsFollowingAsync(int startupId)
        {
            var userId = _currentUserService.GetUserId();
            return await _unitOfWork.StartupFollowers.IsFollowingAsync(userId, startupId);
        }

        public async Task<PagedResult<FollowedStartupResponse>> GetMyFollowedStartupsAsync(SieveModel model)
        {
            var userId = _currentUserService.GetUserId();

            var query = _unitOfWork.StartupFollowers.GetFollowerQuery()
                .Where(sf => sf.FollowerId == userId);

            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, sf => _mapper.Map<FollowedStartupResponse>(sf));
        }
    }
}
