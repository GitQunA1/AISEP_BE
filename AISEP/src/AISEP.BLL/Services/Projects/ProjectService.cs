using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Helpers;
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

            var project = new Project
            {
                StartupId              = startup.StartupId,
                ProjectName            = dto.ProjectName,
                ShortDescription       = dto.ShortDescription,
                DevelopmentStage       = dto.DevelopmentStage,
                ProblemStatement       = dto.ProblemStatement,
                SolutionDescription    = dto.SolutionDescription,
                TargetCustomers        = dto.TargetCustomers,
                UniqueValueProposition = dto.UniqueValueProposition,
                MarketSize             = dto.MarketSize,
                BusinessModel          = dto.BusinessModel,
                Revenue                = dto.Revenue,
                Competitors            = dto.Competitors,
                TeamMembers            = dto.TeamMembers,
                KeySkills              = dto.KeySkills,
                TeamExperience         = dto.TeamExperience,
                Status                 = ProjectStatus.Draft,
                CreatedAt              = DateTime.UtcNow,
                //CreatedBy              = userId

            };

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



            project.ProjectName            = dto.ProjectName            ?? project.ProjectName;
            project.ShortDescription       = dto.ShortDescription       ?? project.ShortDescription;
            project.DevelopmentStage       = dto.DevelopmentStage       ?? project.DevelopmentStage;
            project.ProblemStatement       = dto.ProblemStatement       ?? project.ProblemStatement;
            project.SolutionDescription    = dto.SolutionDescription    ?? project.SolutionDescription;
            project.TargetCustomers        = dto.TargetCustomers        ?? project.TargetCustomers;
            project.UniqueValueProposition = dto.UniqueValueProposition ?? project.UniqueValueProposition;
            project.MarketSize             = dto.MarketSize             ?? project.MarketSize;
            project.BusinessModel          = dto.BusinessModel          ?? project.BusinessModel;
            project.Revenue                = dto.Revenue                ?? project.Revenue;
            project.Competitors            = dto.Competitors            ?? project.Competitors;
            project.TeamMembers            = dto.TeamMembers            ?? project.TeamMembers;
            project.KeySkills              = dto.KeySkills              ?? project.KeySkills;
            project.TeamExperience         = dto.TeamExperience         ?? project.TeamExperience;

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
            //project.PublishedAt = DateTime.UtcNow;
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
    }
}
