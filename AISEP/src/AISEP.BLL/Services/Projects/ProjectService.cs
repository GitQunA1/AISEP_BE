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
using Microsoft.EntityFrameworkCore;
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
        private readonly IStorageService _storage;

        public ProjectService(
            IUnitOfWork unitOfWork,
            ISieveProcessor sieveProcessor,
            IMapper mapper,
            IUserService userService,
            IStorageService storage)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
            _userService = userService;
            _storage = storage;
        }

        public async Task<PagedResult<ProjectResponse>> GetAllProjectsAsync(SieveModel model)
        {
            var currentUserId = GetCurrentUserIdOrNull();
            var currentInvestorId = await GetCurrentInvestorIdOrNullAsync(currentUserId);

            return await PaginationHelper.PaginateAsync(
                _unitOfWork.Projects.GetAllQuery(),
                model,
                _sieveProcessor,
                p => MapProjectResponseWithCurrentUser(p, currentUserId, currentInvestorId));
        }

        public async Task<PagedResult<NonPremiumProjectResponse>> GetAllProjectsForNonPremiumAsync(SieveModel model)
        {
            var currentUserId = GetCurrentUserIdOrNull();
            var currentInvestorId = await GetCurrentInvestorIdOrNullAsync(currentUserId);

            return await PaginationHelper.PaginateAsync(
                _unitOfWork.Projects.GetAllQuery(),
                model,
                _sieveProcessor,
                p => MapNonPremiumProjectResponseWithCurrentUser(p, currentUserId, currentInvestorId));
        }

        public async Task<NonPremiumProjectResponse?> GetProjectForNonPremiumByIdAsync(int id)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            var currentUserId = GetCurrentUserIdOrNull();
            var currentInvestorId = await GetCurrentInvestorIdOrNullAsync(currentUserId);
            return MapNonPremiumProjectResponseWithCurrentUser(project, currentUserId, currentInvestorId);
        }

        public async Task<ProjectResponse?> GetProjectByIdAsync(int id)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            var userId = _userService.GetUserId();
            var role = _userService.GetUserRole();
            var currentInvestorId = await GetCurrentInvestorIdOrNullAsync(userId);

            if (CanBypassViewQuota(project, userId, role))
            {
                return MapProjectResponseWithCurrentUser(project, userId, currentInvestorId);
            }

            if (!RequiresViewQuota(role))
            {
                return MapProjectResponseWithCurrentUser(project, userId, currentInvestorId);
            }

            var isUnlocked = await _unitOfWork.UnlockedProjects.ExistsAsync(userId, id);
            if (!isUnlocked)
            {
                await ConsumeProjectViewQuotaAndUnlockAsync(userId, id);
            }

            return MapProjectResponseWithCurrentUser(project, userId, currentInvestorId);
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
            project.ProjectImageUrl = await UploadIfPresent(dto.ProjectImageFile, "project-images");

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
            if (dto.Industry.HasValue)
                project.Industry = dto.Industry.Value;
            if (dto.ProjectImageFile is not null)
                project.ProjectImageUrl = await _storage.UploadFileAsync(dto.ProjectImageFile, "project-images");

            ValidateByStageLikeCreate(project);

            _unitOfWork.Projects.Update(project);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProjectResponse>(project);
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

        private int? GetCurrentUserIdOrNull()
        {
            if (!_userService.IsAuthenticated())
            {
                return null;
            }

            return _userService.GetUserId();
        }

        private async Task<int?> GetCurrentInvestorIdOrNullAsync(int? currentUserId)
        {
            if (!currentUserId.HasValue)
            {
                return null;
            }

            var currentRole = _userService.GetUserRole();
            if (!string.Equals(currentRole, "Investor", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var investor = await _unitOfWork.Investors.GetByUserIdAsync(currentUserId.Value);
            return investor?.InvestorId;
        }

        private ProjectResponse MapProjectResponseWithCurrentUser(Project project, int? currentUserId, int? currentInvestorId)
        {
            if (currentUserId.HasValue || currentInvestorId.HasValue)
            {
                return _mapper.Map<ProjectResponse>(project, opts =>
                {
                    if (currentUserId.HasValue)
                    {
                        opts.Items["CurrentUserId"] = currentUserId.Value;
                    }

                    if (currentInvestorId.HasValue)
                    {
                        opts.Items["CurrentInvestorId"] = currentInvestorId.Value;
                    }
                });
            }

            return _mapper.Map<ProjectResponse>(project);
        }

        private NonPremiumProjectResponse MapNonPremiumProjectResponseWithCurrentUser(Project project, int? currentUserId, int? currentInvestorId)
        {
            if (currentUserId.HasValue || currentInvestorId.HasValue)
            {
                return _mapper.Map<NonPremiumProjectResponse>(project, opts =>
                {
                    if (currentUserId.HasValue)
                    {
                        opts.Items["CurrentUserId"] = currentUserId.Value;
                    }

                    if (currentInvestorId.HasValue)
                    {
                        opts.Items["CurrentInvestorId"] = currentInvestorId.Value;
                    }
                });
            }

            return _mapper.Map<NonPremiumProjectResponse>(project);
        }

        private static void ValidateByStageLikeCreate(Project project)
        {
            if (!HasValue(project.ProjectName))
                throw new InvalidOperationException("Project name is required.");
            if (!HasValue(project.ShortDescription))
                throw new InvalidOperationException("Short description is required.");
            if (!HasValue(project.ProblemStatement))
                throw new InvalidOperationException("Problem statement is required.");
            if (!HasValue(project.SolutionDescription))
                throw new InvalidOperationException("Solution description is required.");
            if (!HasValue(project.TargetCustomers))
                throw new InvalidOperationException("Target customers is required.");
            if (!HasValue(project.TeamMembers))
                throw new InvalidOperationException("Team members is required.");

            if (project.DevelopmentStage is DevelopmentStage.MVP or DevelopmentStage.Growth)
            {
                if (!HasValue(project.UniqueValueProposition))
                    throw new InvalidOperationException("Unique value proposition is required for MVP and Growth stages.");
                if (!HasValue(project.BusinessModel))
                    throw new InvalidOperationException("Business model is required for MVP and Growth stages.");
                if (!HasValue(project.KeySkills))
                    throw new InvalidOperationException("Key skills are required for MVP and Growth stages.");
                if (!HasValue(project.Competitors))
                    throw new InvalidOperationException("Competitors are required for MVP and Growth stages.");
            }

            if (project.DevelopmentStage == DevelopmentStage.Growth)
            {
                if (project.Revenue is null || project.Revenue <= 0)
                    throw new InvalidOperationException("Revenue must be greater than 0 for Growth stage.");
                if (project.MarketSize is null || project.MarketSize <= 0)
                    throw new InvalidOperationException("Market size must be greater than 0 for Growth stage.");
                if (!HasValue(project.TeamExperience))
                    throw new InvalidOperationException("Team experience is required for Growth stage.");
            }
        }

        private static bool HasValue(string? value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private static string? BuildTeaserText(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = System.Text.RegularExpressions.Regex.Replace(value.Trim(), @"\s+", " ");
            if (normalized.Length <= maxLength)
            {
                return normalized;
            }

            return normalized[..maxLength].TrimEnd() + "...";
        }

        //private NonPremiumProjectResponse MapNonPremiumProject(Project project)
        //{
        //    var response = _mapper.Map<NonPremiumProjectResponse>(project);
        //    response.ProblemStatement = BuildTeaserText(response.ProblemStatement, 220);
        //    response.SolutionDescription = BuildTeaserText(response.SolutionDescription, 220);
        //    response.TargetCustomers = BuildTeaserText(response.TargetCustomers, 120);
        //    response.UniqueValueProposition = BuildTeaserText(response.UniqueValueProposition, 140);
        //    return response;
        //}

        private async Task<string?> UploadIfPresent(IFormFile? file, string folder)
            => file is not null ? await _storage.UploadFileAsync(file, folder) : null;
    }
}
