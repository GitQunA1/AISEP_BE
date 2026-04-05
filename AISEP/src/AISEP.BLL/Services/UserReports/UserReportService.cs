using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Storage;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Sieve.Models;
using Sieve.Services;
using System.Text.Json;

namespace AISEP.BLL.Services.UserReports
{
    public class UserReportService : IUserReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IStorageService _storageService;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;

        public UserReportService(
            IUnitOfWork unitOfWork,
            IUserService userService,
            IStorageService storageService,
            ISieveProcessor sieveProcessor,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _storageService = storageService;
            _sieveProcessor = sieveProcessor;
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

            var uploadedImageUrls = new List<string>();
            if (request.EvidenceImages is not null && request.EvidenceImages.Count > 0)
            {
                foreach (var image in request.EvidenceImages)
                {
                    uploadedImageUrls.Add(await _storageService.UploadFileAsync(image, "user-reports"));
                }
            }

            var report = _mapper.Map<UserReport>(request);
            report.ReporterId = reporterId;
            report.Reason = request.Description.Trim();
            report.EvidenceImageUrls = uploadedImageUrls.Count == 0
                ? null
                : JsonSerializer.Serialize(uploadedImageUrls);
            report.EvidenceUrl = uploadedImageUrls.FirstOrDefault(); // legacy field for compatibility
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

        public async Task<PagedResult<UserReportResponse>> GetUserReports(SieveModel sieveModel)
        {
            //return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, s => _mapper.Map<StartupResponse>(s));
            var query = _unitOfWork.UserReports.GetAll();
            return await PaginationHelper.PaginateAsync(query, sieveModel, _sieveProcessor, s=> _mapper.Map<UserReportResponse>(s));

        }

        public async Task<PagedResult<UserReportResponse>> GetMyReportsAsReporterAsync(SieveModel sieveModel)
        {
            var currentUserId = _userService.GetUserId();
            var query = _unitOfWork.UserReports.GetAll()
                .Where(x => x.ReporterId == currentUserId);

            return await PaginationHelper.PaginateAsync(
                query,
                sieveModel,
                _sieveProcessor,
                x => _mapper.Map<UserReportResponse>(x));
        }

        public async Task<PagedResult<UserReportResponse>> GetMyReportsAsReportedUserAsync(SieveModel sieveModel)
        {
            var currentUserId = _userService.GetUserId();
            var query = _unitOfWork.UserReports.GetAll()
                .Where(x => x.ReportedUserId == currentUserId);

            return await PaginationHelper.PaginateAsync(
                query,
                sieveModel,
                _sieveProcessor,
                x => _mapper.Map<UserReportResponse>(x));
        }
    }
}
