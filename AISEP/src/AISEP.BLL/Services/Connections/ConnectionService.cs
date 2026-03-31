using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Exceptions;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Services.Chats;
using AutoMapper;

namespace AISEP.BLL.Services.Connections
{
    public class ConnectionService : IConnectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly IChatSessionService _chatSessionService;

        public ConnectionService(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IMapper mapper,
            IChatSessionService chatSessionService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _mapper = mapper;
            _chatSessionService = chatSessionService;
        }

        public async Task<ConnectionRequestDto> CreateRequestAsync(int investorId, CreateConnectionRequestDto dto)
        {
            var investor = await _unitOfWork.Investors.GetByIdAsync(investorId)
                ?? throw new KeyNotFoundException("Investor not found.");

            var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId)
                ?? throw new KeyNotFoundException("Project not found.");

            var existing = await _unitOfWork.ConnectionRequests.GetByInvestorAndProjectAsync(investorId, dto.ProjectId);
            if (existing is not null && existing.Status == ConnectionRequestStatus.Pending)
            {
                throw new InvalidOperationException("Connection request is already pending.");
            }

            var request = new ConnectionRequest
            {
                InvestorId = investorId,
                ProjectId = dto.ProjectId,
                Message = dto.Message,
                Status = ConnectionRequestStatus.Pending,
                ResponseDate = null
            };

            await _unitOfWork.ConnectionRequests.AddAsync(request);
            await _unitOfWork.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(
                project.Startup.UserId,
                "New connection request",
                $"Investor has requested contact access for project '{project.ProjectName}'.",
                NotificationType.ConnectionRequest,
                request.ConnectionRequestId,
                "ConnectionRequest");

            return _mapper.Map<ConnectionRequestDto>(request);
        }

        public async Task<ConnectionRequestDto> RespondToRequestAsync(int startupId, int requestId, bool isAccepted)
        {
            var request = await _unitOfWork.ConnectionRequests.GetByIdAsync(requestId)
                ?? throw new KeyNotFoundException("Connection request not found.");

            ChatSessionResponse? openedSession = null;

            if (request.Project.StartupId != startupId)
            {
                throw new ForbiddenAccessException("You do not have permission to respond to this request.");
            }

            if (request.Status != ConnectionRequestStatus.Pending)
            {
                throw new InvalidOperationException("This connection request has already been processed.");
            }

            request.Status = isAccepted ? ConnectionRequestStatus.Accepted : ConnectionRequestStatus.Rejected;
            request.ResponseDate = DateTime.UtcNow;
            _unitOfWork.ConnectionRequests.Update(request);

            if (isAccepted)
            {
                openedSession = await _chatSessionService.OpenSessionByConnectionRequestAsync(
                    request.ConnectionRequestId,
                    request.Project.Startup.UserId);

                if (openedSession is null)
                {
                    throw new InvalidOperationException("Failed to open chat session for accepted connection request.");
                }
            }

            await _unitOfWork.SaveChangesAsync();

            var notifyMessage = isAccepted
                ? $"Your connection request for project '{request.Project.ProjectName}' was accepted."
                : $"Your connection request for project '{request.Project.ProjectName}' was rejected.";

            var investorToNotify = await _unitOfWork.Investors.GetByIdAsync(request.InvestorId)
                ?? throw new KeyNotFoundException("Investor not found.");

            await _notificationService.SendNotificationAsync(
                investorToNotify.UserId,
                "Connection request update",
                notifyMessage,
                NotificationType.ConnectionRequest,
                isAccepted ? openedSession?.ChatSessionId : request.ConnectionRequestId,
                isAccepted ? "ChatSession" : "ConnectionRequest");

            var result = _mapper.Map<ConnectionRequestDto>(request);
            if (isAccepted && openedSession is not null)
            {
                result.ChatSessionId = openedSession.ChatSessionId;
            }

            return result;
        }

        public async Task<ContactInfoDto> GetFounderContactAsync(int investorId, int projectId)
        {
            var hasPermission = await _unitOfWork.ConnectionRequests.ExistsAcceptedAsync(investorId, projectId);
            if (!hasPermission)
            {
                throw new UnauthorizedAccessException("Bị từ chối cấp quyền xem thông tin liên hệ");
            }

            var project = await _unitOfWork.Projects.GetByIdAsync(projectId)
                ?? throw new KeyNotFoundException("Project not found.");

            return _mapper.Map<ContactInfoDto>(project.Startup);
        }
    }
}
