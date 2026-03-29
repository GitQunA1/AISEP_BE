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
            return await PaginationHelper.PaginateAsync(_unitOfWork.Projects.GetAllQuery(), model, _sieveProcessor, p => _mapper.Map<ProjectResponse>(p));
        }

        public async Task<PagedResult<NonPremiumProjectResponse>> GetAllProjectsForNonPremiumAsync(SieveModel model)
        {
            return await PaginationHelper.PaginateAsync(
                _unitOfWork.Projects.GetAllQuery(),
                model,
                _sieveProcessor,
                p => _mapper.Map<NonPremiumProjectResponse>(p));
        }

        public async Task<NonPremiumProjectResponse?> GetProjectForNonPremiumByIdAsync(int id)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            return _mapper.Map<NonPremiumProjectResponse>(project);
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
            await AutoAssignAdvisorAsync(project);
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

        private async Task AutoAssignAdvisorAsync(Project project)
        {
            var advisors = await _unitOfWork.Advisors.GetAllQuery()
                .Where(a => a.ApprovalStatus == ApprovalStatus.Approved
                            && a.Industry == project.Industry)
                .ToListAsync();

            if (advisors.Count == 0)
            {
                return;
            }

            var advisorIds = advisors.Select(a => a.AdvisorId).ToList();
            var today = DateTime.UtcNow.Date;
            var weekEndExclusive = today.AddDays(7);

            var availableCounts = await _unitOfWork.AdvisorAvailabilities.GetQuery()
                .Where(x => advisorIds.Contains(x.AdvisorId)
                            && x.Status == AdvisorAvailabilityStatus.Available
                            && x.SlotDate >= today
                            && x.SlotDate < weekEndExclusive)
                .GroupBy(x => x.AdvisorId)
                .Select(g => new { AdvisorId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AdvisorId, x => x.Count);

            var rejectedCounts = await _unitOfWork.Bookings.GetBookingQuery()
                .Where(b => advisorIds.Contains(b.AdvisorId)
                            && b.Status == BookingStatus.Cancel
                            && b.Note != null
                            && b.Note.Contains("[Advisor Reject]"))
                .GroupBy(b => b.AdvisorId)
                .Select(g => new { AdvisorId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AdvisorId, x => x.Count);

            var noResponseCounts = await _unitOfWork.Bookings.GetBookingQuery()
                .Where(b => advisorIds.Contains(b.AdvisorId)
                            && b.Status == BookingStatus.NoResponse)
                .GroupBy(b => b.AdvisorId)
                .Select(g => new { AdvisorId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AdvisorId, x => x.Count);

            var bestAdvisor = advisors
                .Select(a =>
                {
                    var availability = availableCounts.GetValueOrDefault(a.AdvisorId, 0);
                    var rejected = rejectedCounts.GetValueOrDefault(a.AdvisorId, 0);
                    var noResponse = noResponseCounts.GetValueOrDefault(a.AdvisorId, 0);
                    var rating = (double)(a.Rating ?? 0);

                    var score = availability - (rejected * 2) - (noResponse * 3) + (rating * 0.5);
                    return new { AdvisorId = a.AdvisorId, Score = score, availability, rejected, noResponse };
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.availability)
                .ThenBy(x => x.noResponse)
                .ThenBy(x => x.rejected)
                .First();

            var existingAssignment = await _unitOfWork.ProjectAdvisorAssignments.GetByProjectIdAsync(project.ProjectId);
            if (existingAssignment is null)
            {
                await _unitOfWork.ProjectAdvisorAssignments.AddAsync(new ProjectAdvisorAssignment
                {
                    ProjectId = project.ProjectId,
                    AdvisorId = bestAdvisor.AdvisorId,
                    AssignedAt = DateTime.UtcNow
                });
                return;
            }

            existingAssignment.AdvisorId = bestAdvisor.AdvisorId;
            existingAssignment.AssignedAt = DateTime.UtcNow;
            _unitOfWork.ProjectAdvisorAssignments.Update(existingAssignment);
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
