using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AutoMapper;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.ProjectFollowers
{
    public class ProjectFollowerService : IProjectFollowerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _currentUserService;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;

        public ProjectFollowerService(
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

        public async Task<bool> FollowProjectAsync(int projectId)
        {
            var userId = _currentUserService.GetUserId();
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            var exists = await _unitOfWork.ProjectFollowers.IsFollowingAsync(userId, projectId);
            if (exists)
                return false;

            var follow = new ProjectFollower
            {
                FollowerId = userId,
                ProjectId = projectId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ProjectFollowers.AddAsync(follow);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnfollowProjectAsync(int projectId)
        {
            var userId = _currentUserService.GetUserId();

            var exists = await _unitOfWork.ProjectFollowers.IsFollowingAsync(userId, projectId);
            if (!exists)
                return false;

            await _unitOfWork.ProjectFollowers.RemoveAsync(userId, projectId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsFollowingAsync(int projectId)
        {
            var userId = _currentUserService.GetUserId();
            return await _unitOfWork.ProjectFollowers.IsFollowingAsync(userId, projectId);
        }

        public async Task<PagedResult<FollowedProjectResponse>> GetMyFollowedProjectsAsync(SieveModel model)
        {
            var userId = _currentUserService.GetUserId();

            var query = _unitOfWork.ProjectFollowers.GetFollowerQuery()
                .Where(pf => pf.FollowerId == userId);

            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, pf => _mapper.Map<FollowedProjectResponse>(pf));
        }
    }
}
