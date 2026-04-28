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
            await EnsureUserCanFollowProjectsAsync(userId, requireApprovedActor: true);

            var project = await _unitOfWork.Projects.GetByIdAsync(projectId)
                ?? throw new KeyNotFoundException("Project not found.");

            var exists = await _unitOfWork.ProjectFollowers.IsFollowingAsync(userId, projectId);
            if (exists)
            {
                return false;
            }

            var follow = new ProjectFollower
            {
                FollowerId = userId,
                ProjectId = project.ProjectId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ProjectFollowers.AddAsync(follow);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnfollowProjectAsync(int projectId)
        {
            var userId = _currentUserService.GetUserId();
            await EnsureUserCanFollowProjectsAsync(userId, requireApprovedActor: true);

            var exists = await _unitOfWork.ProjectFollowers.IsFollowingAsync(userId, projectId);
            if (!exists)
            {
                return false;
            }

            await _unitOfWork.ProjectFollowers.RemoveAsync(userId, projectId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsFollowingAsync(int projectId)
        {
            var userId = _currentUserService.GetUserId();
            await EnsureUserCanFollowProjectsAsync(userId);
            return await _unitOfWork.ProjectFollowers.IsFollowingAsync(userId, projectId);
        }

        public async Task<PagedResult<FollowedProjectResponse>> GetMyFollowedProjectsAsync(SieveModel model)
        {
            var userId = _currentUserService.GetUserId();
            await EnsureUserCanFollowProjectsAsync(userId);

            var query = _unitOfWork.ProjectFollowers.GetFollowerQuery()
                .Where(pf => pf.FollowerId == userId);

            return await PaginationHelper.PaginateAsync(
                query,
                model,
                _sieveProcessor,
                pf => _mapper.Map<FollowedProjectResponse>(pf));
        }

        private async Task EnsureUserCanFollowProjectsAsync(int userId, bool requireApprovedActor = false)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            if (user.Role != UserRole.Startup && user.Role != UserRole.Investor)
            {
                throw new ForbiddenAccessException("Only Startup or Investor can follow projects.");
            }

            if (requireApprovedActor && user.Role == UserRole.Startup)
            {
                var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId)
                    ?? throw new KeyNotFoundException("Startup profile not found for this account.");
                if (startup.ApprovalStatus != ApprovalStatus.Approved)
                    throw new InvalidOperationException("Your startup profile must be approved before using this feature.");
            }

            if (requireApprovedActor && user.Role == UserRole.Investor)
            {
                var investor = await _unitOfWork.Investors.GetByUserIdAsync(userId)
                    ?? throw new KeyNotFoundException("Investor profile not found for this account.");
                if (investor.ApprovalStatus != ApprovalStatus.Approved)
                    throw new InvalidOperationException("Your investor profile must be approved before using this feature.");
            }
        }
    }
}
