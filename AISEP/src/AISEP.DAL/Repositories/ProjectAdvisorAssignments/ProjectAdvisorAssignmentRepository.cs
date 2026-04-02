using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.ProjectAdvisorAssignments
{
    public class ProjectAdvisorAssignmentRepository : IProjectAdvisorAssignmentRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectAdvisorAssignmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProjectAdvisorAssignment?> GetByProjectIdAsync(int projectId)
            => await _context.ProjectAdvisorAssignments
                .Include(x => x.Project)
                .Include(x => x.Advisor)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(x => x.ProjectId == projectId);

        public IQueryable<ProjectAdvisorAssignment> GetAllQuery()
            => _context.ProjectAdvisorAssignments
                .Include(x => x.Project)
                .Include(x => x.Advisor)
                    .ThenInclude(a => a.User)
                .OrderByDescending(x => x.AssignedAt)
                .AsQueryable();

        public IQueryable<ProjectAdvisorAssignment> GetByAdvisorIdQuery(int advisorId)
            => _context.ProjectAdvisorAssignments
                .Include(x => x.Project)
                .Include(x => x.Advisor)
                    .ThenInclude(a => a.User)
                .Where(x => x.AdvisorId == advisorId)
                .OrderByDescending(x => x.AssignedAt)
                .AsQueryable();

        public async Task AddAsync(ProjectAdvisorAssignment assignment)
            => await _context.ProjectAdvisorAssignments.AddAsync(assignment);

        public void Update(ProjectAdvisorAssignment assignment)
            => _context.ProjectAdvisorAssignments.Update(assignment);
    }
}
