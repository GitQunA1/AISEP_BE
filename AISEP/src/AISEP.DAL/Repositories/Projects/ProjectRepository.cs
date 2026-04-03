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
                .Include(p => p.StartupAIAnalysis)
                .Include(p => p.Followers)
                .Include(p => p.ConnectionRequests)
                .Include(p => p.ProjectAdvisorAssignment)
                    .ThenInclude(pa => pa.Advisor)
                        .ThenInclude(a => a.User)
                .OrderBy(p => p.ProjectId)
                .AsQueryable();
        }

        public IQueryable<Project> GetByStartupIdQuery(int startupId)
        {
            return _context.Projects
                .Include(p => p.Startup)
                .Include(p => p.StartupAIAnalysis)
                .Include(p => p.Followers)
                .Include(p => p.ConnectionRequests)
                .Include(p => p.ProjectAdvisorAssignment)
                    .ThenInclude(pa => pa.Advisor)
                        .ThenInclude(a => a.User)
                .Where(p => p.StartupId == startupId)
                .OrderBy(p => p.ProjectId)
                .AsQueryable();
        }

        public IQueryable<Project> GetByStatusQuery(ProjectStatus status)
        {
            return _context.Projects
                .Include(p => p.Startup)
                .Include(p => p.StartupAIAnalysis)
                .Include(p => p.Followers)
                .Include(p => p.ConnectionRequests)
                .Include(p => p.ProjectAdvisorAssignment)
                    .ThenInclude(pa => pa.Advisor)
                        .ThenInclude(a => a.User)
                .Where(p => p.Status == status)
                .OrderBy(p => p.ProjectId)
                .AsQueryable();
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.Startup)
                .Include(p => p.StartupAIAnalysis)
                .Include(p => p.Followers)
                .Include(p => p.ConnectionRequests)
                .Include(p => p.ProjectAdvisorAssignment)
                    .ThenInclude(pa => pa.Advisor)
                        .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(p => p.ProjectId == id);
        }

        public async Task<Project?> GetByIdWithDocumentsAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.Startup)
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
