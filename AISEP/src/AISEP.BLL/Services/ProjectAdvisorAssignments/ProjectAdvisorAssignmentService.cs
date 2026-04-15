using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AutoMapper;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.ProjectAdvisorAssignments
{
    public class ProjectAdvisorAssignmentService : IProjectAdvisorAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public ProjectAdvisorAssignmentService(
            IUnitOfWork unitOfWork,
            ISieveProcessor sieveProcessor,
            IMapper mapper,
            IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<List<ProjectAssignedAdvisorResponse>> GetAssignedAdvisorsByProjectAsync(int projectId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId)
                ?? throw new KeyNotFoundException("Project not found.");

            var assignments = await _unitOfWork.ProjectAdvisorAssignments.GetByProjectIdAsync(project.ProjectId);
            if (assignments.Count == 0)
            {
                return [];
            }

            return assignments
                .Select(x => _mapper.Map<ProjectAssignedAdvisorResponse>(x))
                .ToList();
        }

        public async Task<PagedResult<ProjectAssignedAdvisorResponse>> GetAssignedProjectsForCurrentAdvisorAsync(SieveModel model)
        {
            var userId = _userService.GetUserId();
            var advisor = await _unitOfWork.Advisors.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Advisor profile not found for this account.");

            var query = _unitOfWork.ProjectAdvisorAssignments.GetByAdvisorIdQuery(advisor.AdvisorId);

            return await PaginationHelper.PaginateAsync(
                query,
                model,
                _sieveProcessor,
                x => _mapper.Map<ProjectAssignedAdvisorResponse>(x));
        }
    }
}
