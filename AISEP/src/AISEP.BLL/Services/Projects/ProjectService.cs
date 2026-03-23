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

namespace AISEP.BLL.Services.Projects
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public ProjectService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor, IMapper mapper, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<PagedResult<ProjectResponse>> GetAllProjectsAsync(SieveModel model)
        {
            return await PaginationHelper.PaginateAsync(_unitOfWork.Projects.GetAllQuery(), model, _sieveProcessor, p => _mapper.Map<ProjectResponse>(p));
        }

        public async Task<ProjectResponse?> GetProjectByIdAsync(int id)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            var userId = _userService.GetUserId();
            var role = _userService.GetUserRole();

            if (CanBypassViewQuota(project, userId, role))
            {
                return _mapper.Map<ProjectResponse>(project);
            }

            if (!RequiresViewQuota(role))
            {
                return _mapper.Map<ProjectResponse>(project);
            }

            var isUnlocked = await _unitOfWork.UnlockedProjects.ExistsAsync(userId, id);
            if (!isUnlocked)
            {
                await ConsumeProjectViewQuotaAndUnlockAsync(userId, id);
            }

            return _mapper.Map<ProjectResponse>(project);
        }

        public async Task<PagedResult<ProjectResponse>> GetMyProjectsAsync(SieveModel model)
        {
            var userId = _userService.GetUserId();
            var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId);
            if (startup is null)
                throw new KeyNotFoundException("Startup profile not found for this account.");

            return await PaginationHelper.PaginateAsync(_unitOfWork.Projects.GetByStartupIdQuery(startup.StartupId), model, _sieveProcessor, p => _mapper.Map<ProjectResponse>(p));
        }

        public async Task<PagedResult<ProjectResponse>> GetDraftProjectsAsync(SieveModel model)
        {
            return await PaginationHelper.PaginateAsync(_unitOfWork.Projects.GetByStatusQuery(ProjectStatus.Draft), model, _sieveProcessor, p => _mapper.Map<ProjectResponse>(p));
        }

        public async Task<ProjectResponse> CreateProjectAsync( CreateProjectRequest dto)
        {
            var userId = _userService.GetUserId();
            var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId);
            if (startup is null)
                throw new KeyNotFoundException("Startup profile not found. Please create a startup profile first.");

            var project = _mapper.Map<Project>(dto);
            project.StartupId = startup.StartupId;
            project.Industry = dto.Industry!.Value;
            project.Status = ProjectStatus.Draft;
            project.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Projects.AddAsync(project);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProjectResponse>(project);
        }

        public async Task<ProjectResponse> UpdateProjectAsync(int projectId, UpdateProjectRequest dto)
        {
            var userId  = _userService.GetUserId();
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId);
            if (startup is null || project.StartupId != startup.StartupId)
                throw new ForbiddenAccessException("You do not have permission to update this project.");

            if (project.Status != ProjectStatus.Draft && project.Status != ProjectStatus.Rejected)
                throw new InvalidOperationException("Only draft projects or rejected projects can update."); 
            if (project.Status == ProjectStatus.Rejected)
                 project.Status = ProjectStatus.Draft;

            _mapper.Map(dto, project);
            project.Industry = dto.Industry!.Value;

            _unitOfWork.Projects.Update(project);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProjectResponse>(project);
        }

        public async Task ApproveProjectAsync(int projectId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            if (project.Status != ProjectStatus.Pending)
                throw new InvalidOperationException($"Only Pending projects can be approved. Current status: {project.Status}.");

            project.Status = ProjectStatus.Approved;
            project.ApprovedAt = DateTime.UtcNow;
            project.ApprovedById = _userService.GetUserId();
            _unitOfWork.Projects.Update(project);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task SubmitProjectAsync(int projectId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            if (project.Status != ProjectStatus.Draft)
                throw new InvalidOperationException($"Only draft projects can be submitted. Current status: {project.Status}.");

            project.Status      = ProjectStatus.Pending;
           
            _unitOfWork.Projects.Update(project);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RejectProjectAsync(int projectId, RejectProjectRequest dto)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            if (project.Status != ProjectStatus.Pending)
                throw new InvalidOperationException($"Only Pending projects can be rejected. Current status: {project.Status}.");

            project.Status = ProjectStatus.Rejected;
            project.RejectedAt = DateTime.UtcNow;
            project.RejectionReason = dto.Reason?.Trim();
            project.RejectedById = _userService.GetUserId();
            _unitOfWork.Projects.Update(project);
            await _unitOfWork.SaveChangesAsync();
        }

        private static bool CanBypassViewQuota(Project project, int userId, string? role)
        {
            if (string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return project.Startup.UserId == userId;
        }

        private static bool RequiresViewQuota(string? role)
        {
            return string.Equals(role, "Investor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "User", StringComparison.OrdinalIgnoreCase);
        }

        private async Task ConsumeProjectViewQuotaAndUnlockAsync(int userId, int projectId)
        {
            var subscription = await _unitOfWork.Subscriptions.GetLatestActiveAsync(userId)
                ?? throw new InvalidOperationException("No active subscription.");

            var package = await _unitOfWork.Packages.GetByIdAsync(subscription.PackageId)
                ?? throw new KeyNotFoundException("Package not found.");

            if (subscription.UsedProjectViews >= package.MaxProjectViews)
            {
                throw new InvalidOperationException("Bạn đã hết lượt xem dự án. Vui lòng nâng cấp gói.");
            }

            subscription.UsedProjectViews += 1;
            _unitOfWork.Subscriptions.Update(subscription);

            await _unitOfWork.UnlockedProjects.AddAsync(new UnlockedProject
            {
                UserId = userId,
                ProjectId = projectId,
                UnlockedAt = DateTime.UtcNow
            });

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
