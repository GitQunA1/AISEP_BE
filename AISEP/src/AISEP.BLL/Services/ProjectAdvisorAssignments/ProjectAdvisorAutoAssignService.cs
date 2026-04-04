using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace AISEP.BLL.Services.ProjectAdvisorAssignments
{
    public class ProjectAdvisorAutoAssignService : IProjectAdvisorAutoAssignService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProjectAdvisorAutoAssignService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> AutoAssignUnassignedApprovedProjectsAsync(CancellationToken cancellationToken = default)
        {
            var projects = await _unitOfWork.Projects
                .GetByStatusQuery(ProjectStatus.Approved)
                .Where(p => p.ProjectAdvisorAssignment == null)
                .ToListAsync(cancellationToken);

            if (projects.Count == 0)
            {
                return 0;
            }

            var assignedCount = 0;
            foreach (var project in projects)
            {
                var assigned = await TryAssignAdvisorAsync(project, cancellationToken);
                if (assigned)
                {
                    assignedCount++;
                }
            }

            if (assignedCount > 0)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return assignedCount;
        }

        public async Task<bool> TryAssignAdvisorAsync(Project project, CancellationToken cancellationToken = default)
        {
            if (project.Status != ProjectStatus.Approved)
            {
                return false;
            }

            var advisorCandidates = await _unitOfWork.Advisors.GetAllQuery()
                .Where(a => a.ApprovalStatus == ApprovalStatus.Approved
                            && a.AdvisorIndustries.Any(ai => ai.Industry == project.Industry))
                .Select(a => new
                {
                    Advisor = a,
                    AssignedProjectCount = a.ProjectAdvisorAssignments.Count
                })
                .ToListAsync(cancellationToken);

            if (advisorCandidates.Count == 0)
            {
                return false;
            }

            var advisorIds = advisorCandidates.Select(x => x.Advisor.AdvisorId).ToList();
            var today = DateTime.UtcNow.Date;
            var weekEndExclusive = today.AddDays(7);

            var availableCounts = await _unitOfWork.AdvisorAvailabilities.GetQuery()
                .Where(x => advisorIds.Contains(x.AdvisorId)
                            && x.Status == AdvisorAvailabilityStatus.Available
                            && x.SlotDate >= today
                            && x.SlotDate < weekEndExclusive)
                .GroupBy(x => x.AdvisorId)
                .Select(g => new { AdvisorId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AdvisorId, x => x.Count, cancellationToken);

            var rejectedCounts = await _unitOfWork.Bookings.GetBookingQuery()
                .Where(b => advisorIds.Contains(b.AdvisorId)
                            && b.Status == BookingStatus.Cancel
                            && b.Note != null
                            && b.Note.Contains("[Advisor Reject]"))
                .GroupBy(b => b.AdvisorId)
                .Select(g => new { AdvisorId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AdvisorId, x => x.Count, cancellationToken);

            var noResponseCounts = await _unitOfWork.Bookings.GetBookingQuery()
                .Where(b => advisorIds.Contains(b.AdvisorId)
                            && b.Status == BookingStatus.NoResponse)
                .GroupBy(b => b.AdvisorId)
                .Select(g => new { AdvisorId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AdvisorId, x => x.Count, cancellationToken);

            var bestAdvisorId = advisorCandidates
                .Select(x =>
                {
                    var advisorId = x.Advisor.AdvisorId;
                    var availability = availableCounts.GetValueOrDefault(advisorId, 0);
                    var rejected = rejectedCounts.GetValueOrDefault(advisorId, 0);
                    var noResponse = noResponseCounts.GetValueOrDefault(advisorId, 0);
                    var rating = (double)(x.Advisor.Rating ?? 0);
                    var score = availability - (rejected * 2) - (noResponse * 3) + (rating * 0.5);

                    return new
                    {
                        AdvisorId = advisorId,
                        AssignedProjectCount = x.AssignedProjectCount,
                        Score = score,
                        Availability = availability,
                        Rejected = rejected,
                        NoResponse = noResponse
                    };
                })
                .OrderBy(x => x.AssignedProjectCount)
                .ThenByDescending(x => x.Score)
                .ThenByDescending(x => x.Availability)
                .ThenBy(x => x.NoResponse)
                .ThenBy(x => x.Rejected)
                .Select(x => x.AdvisorId)
                .First();

            var existingAssignment = await _unitOfWork.ProjectAdvisorAssignments.GetByProjectIdAsync(project.ProjectId);
            if (existingAssignment is null)
            {
                await _unitOfWork.ProjectAdvisorAssignments.AddAsync(new ProjectAdvisorAssignment
                {
                    ProjectId = project.ProjectId,
                    AdvisorId = bestAdvisorId,
                    AssignedAt = DateTime.UtcNow
                });
                return true;
            }

            if (existingAssignment.AdvisorId == bestAdvisorId)
            {
                return false;
            }

            existingAssignment.AdvisorId = bestAdvisorId;
            existingAssignment.AssignedAt = DateTime.UtcNow;
            _unitOfWork.ProjectAdvisorAssignments.Update(existingAssignment);
            return true;
        }
    }
}
