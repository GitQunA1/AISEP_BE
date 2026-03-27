using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;

namespace AISEP.BLL.Services.UserReports
{
    public class UserReportService : IUserReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserReportService(IUnitOfWork unitOfWork, IUserService userService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<UserReportResponse> CreateAsync(CreateUserReportRequest request)
        {
            var reporterId = _userService.GetUserId();

            if (request.ReportedUserId == reporterId)
            {
                throw new InvalidOperationException("You cannot report yourself.");
            }

            var reportedUser = await _unitOfWork.Users.GetByIdAsync(request.ReportedUserId);
            if (reportedUser is null)
            {
                throw new KeyNotFoundException("Reported user not found.");
            }

            var report = _mapper.Map<UserReport>(request);
            report.ReporterId = reporterId;
            report.Status = UserReportStatus.Pending;
            report.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.UserReports.AddAsync(report);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserReportResponse>(report);
        }

        public async Task<UserReportResponse> ResolveAsValidAsync(int reportId)
        {
            return await UpdateStatusAsync(reportId, UserReportStatus.Resolved);
        }

        public async Task<UserReportResponse> ResolveAsFalseAsync(int reportId)
        {
            return await UpdateStatusAsync(reportId, UserReportStatus.Dismissed);
        }

        private async Task<UserReportResponse> UpdateStatusAsync(int reportId, UserReportStatus newStatus)
        {
            var report = await _unitOfWork.UserReports.GetByIdAsync(reportId);
            if (report is null)
            {
                throw new KeyNotFoundException("User report not found.");
            }

            if (report.Status != UserReportStatus.Pending)
            {
                throw new InvalidOperationException($"Only Pending report can be updated. Current status: {report.Status}.");
            }

            report.Status = newStatus;
            _unitOfWork.UserReports.Update(report);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserReportResponse>(report);
        }
    }
}
