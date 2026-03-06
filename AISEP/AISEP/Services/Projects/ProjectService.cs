using AISEP.Common;
using AISEP.DTOs.Requests;
using AISEP.DTOs.Responses;
using AISEP.Models.Entities;
using AISEP.Models.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.Services.Projects
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;

        public ProjectService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
        }

        public async Task<PagedResult<ProjectResponse>> GetAllProjectsAsync(SieveModel model)
        {
            return await PaginateAsync(_unitOfWork.Projects.GetAllQuery(), model);
        }

        public async Task<ProjectResponse?> GetProjectByIdAsync(int id)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            return project is null ? null : _mapper.Map<ProjectResponse>(project);
        }

        public async Task<PagedResult<ProjectResponse>> GetMyProjectsAsync(int userId, SieveModel model)
        {
            var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId);
            if (startup is null)
                throw new KeyNotFoundException("Startup profile not found for this account.");

            return await PaginateAsync(_unitOfWork.Projects.GetByStartupIdQuery(startup.StartupId), model);
        }

        public async Task<ProjectResponse> CreateProjectAsync(int userId, CreateProjectRequest dto)
        {
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
                CreatedAt              = DateTime.UtcNow
            };

            await _unitOfWork.Projects.AddAsync(project);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProjectResponse>(project);
        }

        public async Task SubmitProjectAsync(int projectId, int userId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId);
            if (startup is null || project.StartupId != startup.StartupId)
                throw new UnauthorizedAccessException("You do not have permission to submit this project.");

            if (project.Status != ProjectStatus.Draft)
                throw new InvalidOperationException($"Only Draft projects can be submitted. Current status: {project.Status}.");

            project.Status = ProjectStatus.Submitted;
            _unitOfWork.Projects.Update(project);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ApproveProjectAsync(int projectId, ApproveProjectRequest dto)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            if (project.Status != ProjectStatus.Submitted)
                throw new InvalidOperationException($"Only Submitted projects can be approved. Current status: {project.Status}.");

            project.Status      = ProjectStatus.Approved;
            project.PublishedAt = DateTime.UtcNow;
            _unitOfWork.Projects.Update(project);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RejectProjectAsync(int projectId, RejectProjectRequest dto)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            if (project.Status != ProjectStatus.Submitted)
                throw new InvalidOperationException($"Only Submitted projects can be rejected. Current status: {project.Status}.");

            project.Status = ProjectStatus.Rejected;
            _unitOfWork.Projects.Update(project);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<PagedResult<ProjectResponse>> PaginateAsync(IQueryable<Project> query, SieveModel model)
        {
            var totalCount = await _sieveProcessor
                .Apply(model, query, applyPagination: false, applySorting: false)
                .CountAsync();

            var items = await _sieveProcessor
                .Apply(model, query)
                .ToListAsync();

            var page     = model.Page ?? 1;
            var pageSize = model.PageSize ?? 10;

            return new PagedResult<ProjectResponse>
            {
                Page       = page,
                PageSize   = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Items      = items.Select(p => _mapper.Map<ProjectResponse>(p))
            };
        }
    }
}
