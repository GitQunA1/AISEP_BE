using AISEP.Data;
using AISEP.Models.Entities;
using AISEP.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AISEP.Repositories.Projects
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
                .AsQueryable();
        }

        public IQueryable<Project> GetByStartupIdQuery(int startupId)
        {
            return _context.Projects
                .Include(p => p.Startup)
                .Where(p => p.StartupId == startupId)
                .AsQueryable();
        }

        public IQueryable<Project> GetByStatusQuery(ProjectStatus status)
        {
            return _context.Projects
                .Include(p => p.Startup)
                .Where(p => p.Status == status)
                .AsQueryable();
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.Startup)
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
