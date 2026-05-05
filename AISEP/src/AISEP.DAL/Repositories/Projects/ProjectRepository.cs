using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.Projects
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Project> GetAllQuery()
        {
            return _context.Projects
                .Include(p => p.Startup)
                .Include(p => p.StageOption)
                .Include(p => p.IndustryOption)
                .Include(p => p.Scorecard)
                .Include(p => p.StartupAIAnalysis)
                .Include(p => p.Followers)
                .Include(p => p.ConnectionRequests)
                .Include(p => p.ProjectAdvisorAssignments)
                    .ThenInclude(pa => pa.Advisor)
                        .ThenInclude(a => a.User)
                .Include(p => p.ProjectAdvisorAssignments)
                    .ThenInclude(pa => pa.Advisor)
                        .ThenInclude(a => a.AdvisorIndustries)
                            .ThenInclude(ai => ai.IndustryOption)
                .OrderBy(p => p.ProjectId)
                .AsQueryable();
        }

        public IQueryable<Project> SearchProjectsQuery(string? query = null)
        {
            var keyword = query?.Trim().ToLower();

            return _context.Projects
                .Include(p => p.Startup)
                .Include(p => p.StageOption)
                .Include(p => p.IndustryOption)
                .Include(p => p.Scorecard)
                .Include(p => p.StartupAIAnalysis)
                .Include(p => p.Followers)
                .Include(p => p.ConnectionRequests)
                .Include(p => p.ProjectAdvisorAssignments)
                    .ThenInclude(pa => pa.Advisor)
                        .ThenInclude(a => a.User)
                .Include(p => p.ProjectAdvisorAssignments)
                    .ThenInclude(pa => pa.Advisor)
                        .ThenInclude(a => a.AdvisorIndustries)
                            .ThenInclude(ai => ai.IndustryOption)
                .Where(p =>
                    p.Status == ProjectStatus.Approved &&
                    (string.IsNullOrWhiteSpace(keyword) ||
                        p.ProjectName.ToLower().Contains(keyword) ||
                        (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(keyword)) ||
                        (p.ProblemStatement != null && p.ProblemStatement.ToLower().Contains(keyword)) ||
                        (p.SolutionDescription != null && p.SolutionDescription.ToLower().Contains(keyword)) ||
                        (p.TargetCustomers != null && p.TargetCustomers.ToLower().Contains(keyword)) ||
                        (p.UniqueValueProposition != null && p.UniqueValueProposition.ToLower().Contains(keyword)) ||
                        (p.BusinessModel != null && p.BusinessModel.ToLower().Contains(keyword)) ||
                        (p.Competitors != null && p.Competitors.ToLower().Contains(keyword)) ||
                        (p.IndustryOption != null && p.IndustryOption.Value.ToLower().Contains(keyword)) ||
                        (p.StageOption != null && p.StageOption.Value.ToLower().Contains(keyword))))
                .OrderBy(p => p.ProjectId)
                .AsQueryable();
        }

        public IQueryable<Project> GetByStartupIdQuery(int startupId)
        {
            return _context.Projects
                .Include(p => p.Startup)
                .Include(p => p.StageOption)
                .Include(p => p.IndustryOption)
                .Include(p => p.Scorecard)
                .Include(p => p.StartupAIAnalysis)
                .Include(p => p.Followers)
                .Include(p => p.ConnectionRequests)
                .Include(p => p.ProjectAdvisorAssignments)
                    .ThenInclude(pa => pa.Advisor)
                        .ThenInclude(a => a.User)
                .Include(p => p.ProjectAdvisorAssignments)
                    .ThenInclude(pa => pa.Advisor)
                        .ThenInclude(a => a.AdvisorIndustries)
                            .ThenInclude(ai => ai.IndustryOption)
                .Where(p => p.StartupId == startupId)
                .OrderBy(p => p.ProjectId)
                .AsQueryable();
        }

        public IQueryable<Project> GetByStatusQuery(ProjectStatus status)
        {
            return _context.Projects
                .Include(p => p.Startup)
                .Include(p => p.StageOption)
                .Include(p => p.IndustryOption)
                .Include(p => p.Scorecard)
                .Include(p => p.StartupAIAnalysis)
                .Include(p => p.Followers)
                .Include(p => p.ConnectionRequests)
                .Include(p => p.ProjectAdvisorAssignments)
                    .ThenInclude(pa => pa.Advisor)
                        .ThenInclude(a => a.User)
                .Include(p => p.ProjectAdvisorAssignments)
                    .ThenInclude(pa => pa.Advisor)
                        .ThenInclude(a => a.AdvisorIndustries)
                            .ThenInclude(ai => ai.IndustryOption)
                .Where(p => p.Status == status)
                .OrderBy(p => p.ProjectId)
                .AsQueryable();
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.Startup)
                .Include(p => p.StageOption)
                .Include(p => p.IndustryOption)
                .Include(p => p.Scorecard)
                .Include(p => p.StartupAIAnalysis)
                .Include(p => p.Followers)
                .Include(p => p.ConnectionRequests)
                .Include(p => p.ProjectAdvisorAssignments)
                    .ThenInclude(pa => pa.Advisor)
                        .ThenInclude(a => a.User)
                .Include(p => p.ProjectAdvisorAssignments)
                    .ThenInclude(pa => pa.Advisor)
                        .ThenInclude(a => a.AdvisorIndustries)
                            .ThenInclude(ai => ai.IndustryOption)
                .FirstOrDefaultAsync(p => p.ProjectId == id);
        }

        public async Task<Project?> GetByIdWithDocumentsAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.Startup)
                .Include(p => p.StageOption)
                .Include(p => p.IndustryOption)
                .Include(p => p.Scorecard)
                .Include(p => p.Documents)
                .FirstOrDefaultAsync(p => p.ProjectId == id);
        }

        public async Task AddAsync(Project project)
        {
            await _context.Projects.AddAsync(project);
        }

        public void Update(Project project)
        {
            _context.Projects.Update(project);
        }
    }
}
