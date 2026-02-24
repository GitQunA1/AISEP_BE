using AISEP.Common;
using AISEP.DTOs;
using AISEP.Models.Entities;
using AISEP.Services.CurrentUser;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.Services.StartupFollowers
{
    public class StartupFollowerService : IStartupFollowerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ISieveProcessor _sieveProcessor;

        public StartupFollowerService(
            IUnitOfWork unitOfWork, 
            ICurrentUserService currentUserService,
            ISieveProcessor sieveProcessor)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _sieveProcessor = sieveProcessor;
        }

        public async Task<bool> FollowStartupAsync(Guid startupId)
        {
            var userId = _currentUserService.GetUserId();
            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

           
            var exists = await _unitOfWork.StartupFollowers.IsFollowingAsync(userId, startupId);
            if (exists)
                return false;

            var follow = new StartupFollower
            {
                UserId = userId,
                StartupId = startupId,
                FollowedAt = DateTime.UtcNow
            };

            await _unitOfWork.StartupFollowers.AddAsync(follow);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnfollowStartupAsync(Guid startupId)
        {
            var userId = _currentUserService.GetUserId();
            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            var exists = await _unitOfWork.StartupFollowers.IsFollowingAsync(userId, startupId);
            if (!exists)
                return false; 

            await _unitOfWork.StartupFollowers.RemoveAsync(userId, startupId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsFollowingAsync(Guid startupId)
        {
            var userId = _currentUserService.GetUserId();
            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            return await _unitOfWork.StartupFollowers.IsFollowingAsync(userId, startupId);
        }

        //public async Task<StartupFollowerResponseDto?> GetFollowerByIdAsync(Guid userId, Guid startupId)
        //{
        //    var follower = await _unitOfWork.StartupFollowers.GetByIdAsync(userId, startupId);
        //    return follower != null ? MapToFollowerDto(follower) : null;
        //}

        public async Task<PagedResultDto<FollowedStartupDto>> GetMyFollowedStartupsAsync(SieveModel model)
        {
            var userId = _currentUserService.GetUserId();
            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            var query = _unitOfWork.StartupFollowers.GetFollowerQuery()
                .Where(sf => sf.UserId == userId);

            return await ApplySieveAndPaginateAsync(query, model);
        }

        //public async Task<PagedResultDto<StartupFollowerResponseDto>> GetStartupFollowersAsync(Guid startupId, SieveModel model)
        //{
        //    var query = _unitOfWork.StartupFollowers.GetFollowerQuery()
        //        .Where(sf => sf.StartupId == startupId);

        //    return await ApplySieveAndPaginateFollowersAsync(query, model);
        //}

        private async Task<PagedResultDto<FollowedStartupDto>> ApplySieveAndPaginateAsync(
            IQueryable<StartupFollower> query, 
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
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResultDto<FollowedStartupDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items.Select(MapToFollowedStartupDto)
            };
        }

        //private async Task<PagedResultDto<StartupFollowerResponseDto>> ApplySieveAndPaginateFollowersAsync(
        //    IQueryable<StartupFollower> query, 
        //    SieveModel sieveModel)
        //{
        //    var totalCount = await _sieveProcessor
        //        .Apply(sieveModel, query, applyPagination: false, applySorting: false)
        //        .CountAsync();

        //    var items = await _sieveProcessor
        //        .Apply(sieveModel, query)
        //        .ToListAsync();

        //    var page = sieveModel.Page ?? 1;
        //    var pageSize = sieveModel.PageSize ?? 10;
        //    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        //    return new PagedResultDto<StartupFollowerResponseDto>
        //    {
        //        Page = page,
        //        PageSize = pageSize,
        //        TotalCount = totalCount,
        //        TotalPages = totalPages,
        //        Items = items.Select(MapToFollowerDto)
        //    };
        //}

        private FollowedStartupDto MapToFollowedStartupDto(StartupFollower sf)
        {
            return new FollowedStartupDto
            {
                StartupId = sf.StartupId,
                CompanyName = sf.Startup?.CompanyName ?? "Unknown",
                LogoUrl = sf.Startup?.LogoUrl,
                Industry = sf.Startup?.Industry,
                FollowedAt = sf.FollowedAt
            };
        }

        //private StartupFollowerResponseDto MapToFollowerDto(StartupFollower sf)
        //{
        //    return new StartupFollowerResponseDto
        //    {
        //        UserId = sf.UserId,
        //        UserName = sf.User?.UserName ?? "Unknown",
        //        StartupId = sf.StartupId,
        //        StartupName = sf.Startup?.CompanyName ?? "Unknown",
        //        FollowedAt = sf.FollowedAt
        //    };
        //}
    }
}
