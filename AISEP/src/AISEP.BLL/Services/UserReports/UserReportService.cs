using System.Text.Json;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Services.Storage;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;

namespace AISEP.BLL.Services.UserReports
{
    public class UserReportService : IUserReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IStorageService _storageService;

        public UserReportService(
            IUnitOfWork unitOfWork,
            IUserService userService,
            IStorageService storageService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _storageService = storageService;
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

            var report = new UserReport
            {
                ReporterId = reporterId,
                ReportedUserId = request.ReportedUserId,
                Category = request.Category,
                Reason = request.Description.Trim(),
                EvidenceImageUrls = uploadedImageUrls.Count == 0
                    ? null
                    : JsonSerializer.Serialize(uploadedImageUrls),
                EvidenceUrl = uploadedImageUrls.FirstOrDefault(), // legacy field for compatibility
                VideoEvidenceUrl = string.IsNullOrWhiteSpace(request.VideoEvidenceUrl)
                    ? null
                    : request.VideoEvidenceUrl.Trim(),
                Status = UserReportStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserReports.AddAsync(report);
            await _unitOfWork.SaveChangesAsync();

            return MapResponse(report);
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

            return MapResponse(report);
        }

        private static UserReportResponse MapResponse(UserReport report)
        {
            return new UserReportResponse
            {
                UserReportId = report.UserReportId,
                ReporterId = report.ReporterId,
                ReportedUserId = report.ReportedUserId,
                Category = report.Category.ToString(),
                Description = report.Reason,
                EvidenceImageUrls = ParseEvidenceImageUrls(report.EvidenceImageUrls, report.EvidenceUrl),
                VideoEvidenceUrl = report.VideoEvidenceUrl,
                Status = report.Status.ToString(),
                CreatedAt = report.CreatedAt
            };
        }

        private static List<string> ParseEvidenceImageUrls(string? evidenceImageUrlsJson, string? legacyEvidenceUrl)
        {
            if (!string.IsNullOrWhiteSpace(evidenceImageUrlsJson))
            {
                try
                {
                    var urls = JsonSerializer.Deserialize<List<string>>(evidenceImageUrlsJson);
                    if (urls is not null && urls.Count > 0)
                    {
                        return urls;
                    }
                }
                catch
                {
                    // fallback to legacy url
                }
            }

            return string.IsNullOrWhiteSpace(legacyEvidenceUrl)
                ? []
                : [legacyEvidenceUrl];
        }
    }
}
