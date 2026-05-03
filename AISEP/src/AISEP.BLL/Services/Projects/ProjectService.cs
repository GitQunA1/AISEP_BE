using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Storage;
using AISEP.BLL.Services.Users;
using AISEP.BLL.Services.FormValidationRules;
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
        private const int RequiredProjectIndustries = 1;

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IStorageService _storage;
        private readonly IDynamicFormSubmissionValidationService _dynamicFormValidationService;

        public ProjectService(
            IUnitOfWork unitOfWork,
            ISieveProcessor sieveProcessor,
            IMapper mapper,
            IUserService userService,
            IStorageService storage,
            IDynamicFormSubmissionValidationService dynamicFormValidationService)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
            _userService = userService;
            _storage = storage;
            _dynamicFormValidationService = dynamicFormValidationService;
        }

        // lấy danh sách project cho tk bth
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

       // lấy danh sách project cho tk non-premium
        public async Task<PagedResult<NonPremiumProjectResponse>> GetAllProjectsForNonPremiumAsync(SieveModel model)
        {
            var currentUserId = GetCurrentUserIdOrNull();
            var currentInvestorId = await GetCurrentInvestorIdOrNullAsync(currentUserId);

            var pagedResult = await PaginationHelper.PaginateAsync(
                _unitOfWork.Projects.GetAllQuery(),
                model,
                _sieveProcessor,
                p => MapNonPremiumProjectResponseWithCurrentUser(p, currentUserId, currentInvestorId));

            if (!currentUserId.HasValue)
            {
                return pagedResult;
            }
            //kiểm tra từng project trong danh sách hiện tại xem project đó có được user này unlock chưa
            var items = pagedResult.Items.ToList();
            foreach (var item in items)
            {
                item.IsUnlockedByCurrentUser = await _unitOfWork.UnlockedProjects.ExistsAsync(currentUserId.Value, item.ProjectId);
            }

            pagedResult.Items = items;
            return pagedResult;
        }

       
        public async Task<NonPremiumProjectResponse?> GetProjectForNonPremiumByIdAsync(int id)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            var currentUserId = GetCurrentUserIdOrNull();
            var currentInvestorId = await GetCurrentInvestorIdOrNullAsync(currentUserId);

            var response = MapNonPremiumProjectResponseWithCurrentUser(project, currentUserId, currentInvestorId);
            response.IsUnlockedByCurrentUser = currentUserId.HasValue
                && await _unitOfWork.UnlockedProjects.ExistsAsync(currentUserId.Value, project.ProjectId);

            return response;
        }

        // Lấy chi tiết project theo id, đồng thời xử lý logic quota/unlock nếu cần.
        public async Task<ProjectResponse?> GetProjectByIdAsync(int id)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            var userId = _userService.GetUserId();
            var role = _userService.GetUserRole();
            var currentInvestorId = await GetCurrentInvestorIdOrNullAsync(userId);
            //Check xem user này có được bỏ qua giới hạn lượt xem không
            //Nếu được bypass thì trả project luôn, không trừ quota, không cần unlock.
            if (CanBypassViewQuota(project, userId, role))
            {
                return MapProjectResponseWithCurrentUser(project, userId, currentInvestorId);
            }
            //Check role này có cần áp dụng quota xem project không.
            if (!RequiresViewQuota(role))
            {
                return MapProjectResponseWithCurrentUser(project, userId, currentInvestorId);
            }
            //check đã unclock project này chưa
            //Trừ quota/lượt xem của user
            //Sau đó unlock project này cho user

            var isUnlocked = await _unitOfWork.UnlockedProjects.ExistsAsync(userId, id);
            if (!isUnlocked)
            {
                await ConsumeProjectViewQuotaAndUnlockAsync(userId, id);
            }

            return MapProjectResponseWithCurrentUser(project, userId, currentInvestorId);
        }

        // Lấy danh sách project của startup hiện tại.
        public async Task<PagedResult<ProjectResponse>> GetMyProjectsAsync(SieveModel model)
        {
            var userId = _userService.GetUserId();
            var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId);
            if (startup is null)
                throw new KeyNotFoundException("Startup profile not found for this account.");

            return await PaginationHelper.PaginateAsync(_unitOfWork.Projects.GetByStartupIdQuery(startup.StartupId), model, _sieveProcessor, p => _mapper.Map<ProjectResponse>(p));
        }

        // Tạo project mới
        public async Task<ProjectResponse> CreateProjectAsync( CreateProjectRequest dto)
        {
            // validate theo rule trong DB.
            await _dynamicFormValidationService.ValidateAsync("project.create", dto);

            var userId = _userService.GetUserId();
            var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId);
            if (startup is null)
                throw new KeyNotFoundException("Startup profile not found. Please create a startup profile first.");
            if (startup.ApprovalStatus != ApprovalStatus.Approved)
                throw new InvalidOperationException("Your startup profile must be approved before using this feature.");

            var industryOptions = await ResolveIndustryOptionsAsync(dto.IndustryOptionIds);
            var stageOption = await ResolveStageOptionAsync(dto.StageOptionId, true);
            var project = _mapper.Map<Project>(dto);
            project.StartupId = startup.StartupId;
            project.StageOptionId = stageOption!.Id;
            project.StageOption = stageOption;
            project.Status = ProjectStatus.Draft;
            project.CreatedAt = DateTime.UtcNow;
            project.ProjectImageUrl = await UploadIfPresent(dto.ProjectImageFile, "project-images");
            project.ProjectIndustries = industryOptions
                .Select(option => new ProjectIndustry
                {
                    IndustryOptionId = option.Id,
                    IndustryOption = option
                })
                .ToList();
            await _unitOfWork.Projects.AddAsync(project);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProjectResponse>(project);
        }

        // Cập nhật project
        public async Task<ProjectResponse> UpdateProjectAsync(int projectId, UpdateProjectRequest dto)
        {
            // project.update cũng validate field-level từ DB trước khi patch dữ liệu vào entity.
            await _dynamicFormValidationService.ValidateAsync("project.update", dto);

            var userId  = _userService.GetUserId();
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId);
            if (startup is null || project.StartupId != startup.StartupId)
                throw new ForbiddenAccessException("You do not have permission to update this project.");
            if (startup.ApprovalStatus != ApprovalStatus.Approved)
                throw new InvalidOperationException("Your startup profile must be approved before using this feature.");

            if (project.Status != ProjectStatus.Draft && project.Status != ProjectStatus.Rejected)
                throw new InvalidOperationException("Only draft projects or rejected projects can update."); 
            if (project.Status == ProjectStatus.Rejected)
                 project.Status = ProjectStatus.Draft;

            _mapper.Map(dto, project);
            if (dto.StageOptionId.HasValue)
            {
                var stageOption = await ResolveStageOptionAsync(dto.StageOptionId.Value, true);
                project.StageOptionId = stageOption!.Id;
                project.StageOption = stageOption;
            }
            if (dto.IndustryOptionIds is not null)
            {
                var industryOptions = await ResolveIndustryOptionsAsync(dto.IndustryOptionIds);
                SyncProjectIndustries(project, industryOptions);
            }
            if (dto.ProjectImageFile is not null)
                project.ProjectImageUrl = await _storage.UploadFileAsync(dto.ProjectImageFile, "project-images");
            if (dto.ProjectName is not null)
                project.ProjectName = dto.ProjectName.Trim();
            if (dto.ShortDescription is not null)
                project.ShortDescription = dto.ShortDescription.Trim();
            if (dto.ProblemStatement is not null)
                project.ProblemStatement = dto.ProblemStatement.Trim();
            if (dto.SolutionDescription is not null)
                project.SolutionDescription = dto.SolutionDescription.Trim();
            if (dto.TargetCustomers is not null)
                project.TargetCustomers = dto.TargetCustomers.Trim();
            if (dto.UniqueValueProposition is not null)
                project.UniqueValueProposition = dto.UniqueValueProposition.Trim();
            if (dto.BusinessModel is not null)
                project.BusinessModel = dto.BusinessModel.Trim();
            if (dto.Competitors is not null)
                project.Competitors = dto.Competitors.Trim();
            if (dto.TeamMembers is not null)
                project.TeamMembers = dto.TeamMembers.Trim();
            if (dto.KeySkills is not null)
                project.KeySkills = dto.KeySkills.Trim();
            if (dto.TeamExperience is not null)
                project.TeamExperience = dto.TeamExperience.Trim();

            _unitOfWork.Projects.Update(project);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProjectResponse>(project);
        }

        // Chuyển project từ Draft sang Pending để chờ duyệt.
        public async Task SubmitProjectAsync(int projectId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");
            var userId = _userService.GetUserId();
            var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Startup profile not found for this account.");
            if (project.StartupId != startup.StartupId)
                throw new ForbiddenAccessException("You do not have permission to submit this project.");
            if (startup.ApprovalStatus != ApprovalStatus.Approved)
                throw new InvalidOperationException("Your startup profile must be approved before using this feature.");

            if (project.Status != ProjectStatus.Draft)
                throw new InvalidOperationException($"Only draft projects can be submitted. Current status: {project.Status}.");

            project.Status      = ProjectStatus.Pending;
           
            _unitOfWork.Projects.Update(project);
            await _unitOfWork.SaveChangesAsync();
        }

        // Từ chối project đang ở trạng thái Pending.
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

        // Xác định user hiện tại có được bypass quota xem project hay không.
        private static bool CanBypassViewQuota(Project project, int userId, string? role)
        {
            // Bypass nếu user có role Staff/Admin/Advisor.
            if (string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "Advisor", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            // Bypass nếu user là chủ sở hữu của project đó.
            return project.Startup.UserId == userId;
        }

        // Xác định role hiện tại có phải tiêu hao quota khi xem project hay không.
        private static bool RequiresViewQuota(string? role)
        {
            return string.Equals(role, "Investor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "Startup", StringComparison.OrdinalIgnoreCase);
                
        }

        // Trừ quota xem project và tạo bản ghi unlock cho user hiện tại.
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

        // Lấy user id hiện tại hoặc null nếu chưa đăng nhập.
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

            //var currentRole = _userService.GetUserRole();
            //if (!string.Equals(currentRole, "Investor", StringComparison.OrdinalIgnoreCase))
            //{
            //    return null;
            //}

            var investor = await _unitOfWork.Investors.GetByUserIdAsync(currentUserId.Value);
            return investor?.InvestorId;
        }

        // Map Project sang response 
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

        // Map Project sang response non-premium
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

       

        // Upload file nếu request có gửi ảnh project lên.
        private async Task<string?> UploadIfPresent(IFormFile? file, string folder)
            => file is not null ? await _storage.UploadFileAsync(file, folder) : null;

        // Kiểm tra danh sách ngành của project có tồn tại và đang active hay không.
        private async Task<List<IndustryOption>> ResolveIndustryOptionsAsync(IEnumerable<int>? optionIds)
        {
            var ids = optionIds?
                .Distinct()
                .ToList() ?? [];

            if (ids.Count == 0)
            {
                throw new InvalidOperationException("At least one industry is required.");
            }
            if (ids.Count != RequiredProjectIndustries)
            {
                throw new InvalidOperationException("Project must select exactly one industry.");
            }

            var options = await _unitOfWork.IndustryOptions.GetByIdsAsync(ids);
            if (options.Count != ids.Count || options.Any(x => !x.IsActive))
            {
                throw new InvalidOperationException("One or more selected industries are invalid or inactive.");
            }

            return options;
        }

        // Kiểm tra stage của project có tồn tại và đang active hay không.
        private async Task<StageOption?> ResolveStageOptionAsync(int? stageOptionId, bool required)
        {
            if (!stageOptionId.HasValue)
            {
                if (required)
                {
                    throw new InvalidOperationException("Stage is required.");
                }

                return null;
            }

            var option = await _unitOfWork.StageOptions.GetByIdAsync(stageOptionId.Value);
            if (option is null || !option.IsActive)
            {
                throw new InvalidOperationException("Selected stage is invalid or inactive.");
            }

            return option;
        }

        // Đồng bộ bảng project_industries với danh sách ngành mới nhất.
        private static void SyncProjectIndustries(Project project, IEnumerable<IndustryOption> industryOptions)
        {
            var requestedOptions = industryOptions.ToList();
            var requestedIds = requestedOptions.Select(x => x.Id).ToHashSet();
            if (requestedIds.Count == 0)
            {
                throw new InvalidOperationException("At least one industry is required.");
            }
            if (requestedIds.Count != RequiredProjectIndustries)
            {
                throw new InvalidOperationException("Project must select exactly one industry.");
            }

            var toRemove = project.ProjectIndustries
                .Where(x => !requestedIds.Contains(x.IndustryOptionId))
                .ToList();

            foreach (var item in toRemove)
            {
                project.ProjectIndustries.Remove(item);
            }

            var currentIds = project.ProjectIndustries
                .Select(x => x.IndustryOptionId)
                .ToHashSet();

            foreach (var option in requestedOptions.Where(x => !currentIds.Contains(x.Id)))
            {
                project.ProjectIndustries.Add(new ProjectIndustry
                {
                    ProjectId = project.ProjectId,
                    IndustryOptionId = option.Id,
                    IndustryOption = option
                });
            }
        }
    }
}
