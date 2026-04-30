using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AISEP.BLL.Services.FormValidationRules
{
    public class FormValidationRuleService : IFormValidationRuleService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;

        public FormValidationRuleService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
        }

        // Lấy bộ rule của một form key có hỗ trợ filter, sort và phân trang bằng Sieve.
        public async Task<PagedResult<FormValidationRuleResponse>> GetByFormKeyAsync(string formKey, SieveModel model)
        {
            var normalizedFormKey = NormalizeKey(formKey, nameof(formKey));
            var query = _unitOfWork.FormValidationRules.GetAllQuery()
                .AsNoTracking()
                .Where(x => x.FormKey == normalizedFormKey);

            return await PaginationHelper.PaginateAsync(
                query,
                model,
                _sieveProcessor,
                MapToResponse);
        }

        // Tạo mới một rule. Nếu rule của FormKey và FieldKey đã tồn tại thì báo conflict.
        public async Task<FormValidationRuleResponse> CreateAsync(CreateFormValidationRuleRequest request)
        {
            var normalizedFormKey = NormalizeKey(request.FormKey, nameof(request.FormKey));
            var normalizedFieldKey = NormalizeKey(request.FieldKey, nameof(request.FieldKey));

            await ValidateRequestAsync(request);

            var rule = await _unitOfWork.FormValidationRules.GetByFormAndFieldAsync(normalizedFormKey, normalizedFieldKey);
            if (rule is not null)
            {
                throw new InvalidOperationException("A validation rule for this form and field already exists.");
            }

            rule = new FormValidationRule
            {
                FormKey = normalizedFormKey,
                FieldKey = normalizedFieldKey,
                CreatedAt = DateTime.UtcNow
            };

            ApplyRequest(rule, request);
            await _unitOfWork.FormValidationRules.AddAsync(rule);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(rule);
        }

        // Chỉ cập nhật một rule đã tồn tại sẵn theo id.
        public async Task<FormValidationRuleResponse> UpdateAsync(int id, UpsertFormValidationRuleRequest request)
        {
            await ValidateRequestAsync(request);

            var rule = await _unitOfWork.FormValidationRules.GetByIdAsync(id);
            if (rule is null)
            {
                throw new KeyNotFoundException("Validation rule not found.");
            }

            ApplyRequest(rule, request);
            _unitOfWork.FormValidationRules.Update(rule);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(rule);
        }

        // Kiểm tra cấu hình rule trước khi lưu, tránh tạo ra dữ liệu validate mâu thuẫn hoặc không dùng được.
        private async Task ValidateRequestAsync(UpsertFormValidationRuleRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.CustomRegexPattern))
            {
                try
                {
                    _ = new Regex(request.CustomRegexPattern, RegexOptions.None, TimeSpan.FromSeconds(1));
                }
                catch (ArgumentException ex)
                {
                    throw new InvalidOperationException($"CustomRegexPattern is invalid: {ex.Message}");
                }
            }

            if (request.MinLength.HasValue && request.MinLength < 0)
            {
                throw new InvalidOperationException("MinLength cannot be negative.");
            }

            if (request.MaxLength.HasValue && request.MaxLength < 0)
            {
                throw new InvalidOperationException("MaxLength cannot be negative.");
            }

            if (request.MinLength.HasValue && request.MaxLength.HasValue && request.MinLength > request.MaxLength)
            {
                throw new InvalidOperationException("MinLength cannot be greater than MaxLength.");
            }

            if (request.MinValue.HasValue && request.MaxValue.HasValue && request.MinValue > request.MaxValue)
            {
                throw new InvalidOperationException("MinValue cannot be greater than MaxValue.");
            }

            if (request.AllowedFileTypes is not null)
            {
                var invalidTypes = request.AllowedFileTypes
                    .Where(string.IsNullOrWhiteSpace)
                    .ToList();

                if (invalidTypes.Count != 0)
                {
                    throw new InvalidOperationException("AllowedFileTypes cannot contain empty values.");
                }
            }

            if (request.StageOptionIds is not null && request.StageOptionIds.Any(id => id <= 0))
            {
                throw new InvalidOperationException("StageOptionIds must contain positive stage option ids.");
            }

            if (request.StageOptionIds is { Count: > 0 })
            {
                var requestedIds = request.StageOptionIds
                    .Distinct()
                    .ToList();
                var stageOptions = await _unitOfWork.StageOptions.GetByIdsAsync(requestedIds);

                if (stageOptions.Count != requestedIds.Count || stageOptions.Any(x => !x.IsActive))
                {
                    throw new InvalidOperationException("One or more required stage options are invalid or inactive.");
                }
            }
        }

        // Áp dữ liệu từ request vào entity rule để lưu xuống DB.
        private static void ApplyRequest(FormValidationRule rule, UpsertFormValidationRuleRequest request)
        {
            rule.IsRequired = request.IsRequired;
            rule.MinLength = request.MinLength;
            rule.MaxLength = request.MaxLength;
            rule.CustomRegexPattern = NormalizeNullableValue(request.CustomRegexPattern);
            rule.MinValue = request.MinValue;
            rule.MaxValue = request.MaxValue;
            rule.AllowedFileTypesJson = request.AllowedFileTypes is { Count: > 0 }
                ? JsonSerializer.Serialize(
                    request.AllowedFileTypes
                        .Select(x => x.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList(),
                    JsonOptions)
                : null;
            rule.StageOptions.Clear();
            if (request.StageOptionIds is { Count: > 0 })
            {
                foreach (var stageOptionId in request.StageOptionIds.Where(id => id > 0).Distinct())
                {
                    rule.StageOptions.Add(new FormValidationRuleStageOption
                    {
                        StageOptionId = stageOptionId
                    });
                }
            }

            rule.MaxFileSizeBytes = request.MaxFileSizeBytes > 0
                ? request.MaxFileSizeBytes
                : null;
            rule.UpdatedAt = DateTime.UtcNow;
        }

        // Chuyển entity DB sang response DTO để trả API.
        private static FormValidationRuleResponse MapToResponse(FormValidationRule rule)
        {
            List<string>? allowedFileTypes = null;
            if (!string.IsNullOrWhiteSpace(rule.AllowedFileTypesJson))
            {
                allowedFileTypes = JsonSerializer.Deserialize<List<string>>(rule.AllowedFileTypesJson, JsonOptions);
            }

            var stageOptionIds = rule.StageOptions
                .Select(x => x.StageOptionId)
                .OrderBy(id => id)
                .ToList();

            return new FormValidationRuleResponse
            {
                Id = rule.Id,
                FieldKey = rule.FieldKey,
                IsRequired = rule.IsRequired,
                MinLength = rule.MinLength,
                MaxLength = rule.MaxLength,
                CustomRegexPattern = rule.CustomRegexPattern,
                MinValue = rule.MinValue,
                MaxValue = rule.MaxValue,
                AllowedFileTypes = allowedFileTypes,
                StageOptionIds = stageOptionIds.Count > 0 ? stageOptionIds : null,
                MaxFileSizeBytes = rule.MaxFileSizeBytes,
                UpdatedAt = rule.UpdatedAt
            };
        }

        // Chuẩn hóa giá trị key bắt buộc như formKey hoặc fieldKey.
        private static string NormalizeKey(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"{parameterName} is required.");
            }

            return value.Trim();
        }

        // Chuẩn hóa giá trị tùy chọn: rỗng thì trả null, có dữ liệu thì trim.
        private static string? NormalizeNullableValue(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
