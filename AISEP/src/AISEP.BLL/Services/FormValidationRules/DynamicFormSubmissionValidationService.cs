using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using FluentValidation;
using FluentValidation.Results;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AISEP.BLL.Services.FormValidationRules
{
    public class DynamicFormSubmissionValidationService : IDynamicFormSubmissionValidationService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly IUnitOfWork _unitOfWork;

        public DynamicFormSubmissionValidationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Validate request create/update theo rule dong trong DB truoc khi service nghiep vu xu ly tiep.
        public async Task ValidateAsync(string formKey, object request)
        {
            // Luc submit se doc rule moi nhat tu DB thay vi dung validator hard-code cu.
            var rules = await _unitOfWork.FormValidationRules.GetByFormKeyAsync(formKey.Trim());
            if (rules.Count == 0)
            {
                return;
            }

            var failures = new List<ValidationFailure>();
            var propertyMap = BuildPropertyMap(request.GetType());

            foreach (var rule in rules)
            {
                // FieldKey trong DB se map sang property cua DTO theo ten goc hoac camelCase.
                if (!propertyMap.TryGetValue(rule.FieldKey, out var property))
                {
                    continue;
                }

                var value = property.GetValue(request);
                ValidateRequired(rule, value, failures, IsRequired(rule, request, propertyMap));

                if (value is null || IsWhitespaceString(value))
                {
                    continue;
                }

                switch (value)
                {
                    case string text:
                        ValidateString(rule, text, failures);
                        break;
                    case IFormFile file:
                        ValidateFile(rule, file, failures);
                        break;
                    case IEnumerable enumerable when value is not string:
                        ValidateEnumerable(rule, property, enumerable, failures);
                        break;
                    default:
                        ValidateScalar(rule, property, value, failures);
                        break;
                }
            }

            if (failures.Count > 0)
            {
                // Middleware se convert ValidationException nay thanh response 400 chuan cua API.
                throw new ValidationException(failures);
            }
        }

        // Tao map giua ten field trong DB va property cua DTO de tim field can validate.
        private static Dictionary<string, PropertyInfo> BuildPropertyMap(Type type)
        {
            var dictionary = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                dictionary[property.Name] = property;
                var camelCase = char.ToLowerInvariant(property.Name[0]) + property.Name[1..];
                dictionary[camelCase] = property;
            }

            return dictionary;
        }

        // Kiem tra rule bat buoc cho tung field.
        private static void ValidateRequired(FormValidationRule rule, object? value, List<ValidationFailure> failures, bool isRequired)
        {
            if (!isRequired)
            {
                return;
            }

            if (value is null)
            {
                failures.Add(new ValidationFailure(rule.FieldKey, $"{rule.FieldKey} là bắt buộc."));
                return;
            }

            if (value is string text && string.IsNullOrWhiteSpace(text))
            {
                failures.Add(new ValidationFailure(rule.FieldKey, $"{rule.FieldKey} là bắt buộc."));
                return;
            }

            if (value is IEnumerable enumerable && value is not string && !enumerable.Cast<object?>().Any())
            {
                failures.Add(new ValidationFailure(rule.FieldKey, $"{rule.FieldKey} là bắt buộc."));
            }
        }

        private static bool IsRequired(
            FormValidationRule rule,
            object request,
            Dictionary<string, PropertyInfo> propertyMap)
        {
            var stageOptionIds = rule.StageOptions
                .Select(x => x.StageOptionId)
                .ToList();

            if (stageOptionIds.Count == 0)
            {
                return rule.IsRequired;
            }

            if (!propertyMap.TryGetValue("stageOptionId", out var stageOptionProperty))
            {
                return false;
            }

            var stageOptionId = stageOptionProperty.GetValue(request);
            if (!TryConvertToInt(stageOptionId, out var selectedStageOptionId))
            {
                return false;
            }

            var stageIsInList = stageOptionIds.Contains(selectedStageOptionId);
            return stageIsInList ? rule.IsRequired : !rule.IsRequired;
        }

        // Validate field kieu text: do dai va regex custom.
        private static void ValidateString(FormValidationRule rule, string text, List<ValidationFailure> failures)
        {
            var trimmed = text.Trim();

            if (rule.MinLength.HasValue && trimmed.Length < rule.MinLength.Value)
            {
                failures.Add(new ValidationFailure(rule.FieldKey, $"{rule.FieldKey} phải có ít nhất {rule.MinLength.Value} ký tự."));
            }

            if (rule.MaxLength.HasValue && trimmed.Length > rule.MaxLength.Value)
            {
                failures.Add(new ValidationFailure(rule.FieldKey, $"{rule.FieldKey} không được vượt quá {rule.MaxLength.Value} ký tự."));
            }

            ValidatePattern(rule, trimmed, failures);
        }

        // Validate field kieu file: dung luong va dinh dang file.
        private static void ValidateFile(FormValidationRule rule, IFormFile file, List<ValidationFailure> failures)
        {
            if (rule.MaxFileSizeBytes.HasValue && file.Length > rule.MaxFileSizeBytes.Value)
            {
                failures.Add(new ValidationFailure(rule.FieldKey, $"{rule.FieldKey} không được vượt quá {rule.MaxFileSizeBytes.Value} byte."));
            }

            if (!string.IsNullOrWhiteSpace(rule.AllowedFileTypesJson))
            {
                var allowedTypes = JsonSerializer.Deserialize<List<string>>(rule.AllowedFileTypesJson, JsonOptions) ?? [];
                if (allowedTypes.Count > 0 && !allowedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
                {
                    failures.Add(new ValidationFailure(rule.FieldKey, $"{rule.FieldKey} có loại tệp không được hỗ trợ."));
                }
            }
        }

        // Validate trường kiểu danh sách, ví dụ danh sách giá trị liệt kê như industries.
        private static void ValidateEnumerable(FormValidationRule rule, PropertyInfo property, IEnumerable enumerable, List<ValidationFailure> failures)
        {
            var items = enumerable.Cast<object?>().ToList();
            var count = items.Count;

            if (rule.MinLength.HasValue && count < rule.MinLength.Value)
            {
                failures.Add(new ValidationFailure(rule.FieldKey, $"{rule.FieldKey} phải có ít nhất {rule.MinLength.Value} mục."));
            }

            if (rule.MaxLength.HasValue && count > rule.MaxLength.Value)
            {
                failures.Add(new ValidationFailure(rule.FieldKey, $"{rule.FieldKey} không được vượt quá {rule.MaxLength.Value} mục."));
            }

            if (IsEnumerableOfEnum(property.PropertyType)
                && items.Where(x => x is not null).Any(x => x!.GetType().IsEnum && !Enum.IsDefined(x.GetType(), x)))
            {
                failures.Add(new ValidationFailure(rule.FieldKey, $"{rule.FieldKey} có giá trị enum không hợp lệ."));
            }
        }

        // Validate trường kiểu số hoặc giá trị liệt kê đơn.
        private static void ValidateScalar(FormValidationRule rule, PropertyInfo property, object value, List<ValidationFailure> failures)
        {
            if (IsEnumType(property.PropertyType) && value.GetType().IsEnum)
            {
                if (!Enum.IsDefined(value.GetType(), value))
                {
                    failures.Add(new ValidationFailure(rule.FieldKey, $"{rule.FieldKey} có giá trị enum không hợp lệ."));
                }

                return;
            }

            if (!TryConvertToDecimal(value, out var numericValue))
            {
                return;
            }

            if (rule.MinValue.HasValue && numericValue < rule.MinValue.Value)
            {
                failures.Add(new ValidationFailure(rule.FieldKey, $"{rule.FieldKey} phải lớn hơn hoặc bằng {rule.MinValue.Value}."));
            }

            if (rule.MaxValue.HasValue && numericValue > rule.MaxValue.Value)
            {
                failures.Add(new ValidationFailure(rule.FieldKey, $"{rule.FieldKey} phải nhỏ hơn hoặc bằng {rule.MaxValue.Value}."));
            }
        }

        // Validate theo regex custom duoc cau hinh truc tiep trong DB.
        private static void ValidatePattern(FormValidationRule rule, string text, List<ValidationFailure> failures)
        {
            if (string.IsNullOrWhiteSpace(rule.CustomRegexPattern))
            {
                return;
            }

            if (!Regex.IsMatch(text, rule.CustomRegexPattern, RegexOptions.None, TimeSpan.FromSeconds(1)))
            {
                failures.Add(new ValidationFailure(rule.FieldKey, $"{rule.FieldKey} không đúng định dạng yêu cầu."));
            }
        }

        // Co gang chuyen cac kieu so pho bien ve decimal de ap min/max value.
        private static bool TryConvertToDecimal(object value, out decimal decimalValue)
        {
            switch (value)
            {
                case decimal d:
                    decimalValue = d;
                    return true;
                case int i:
                    decimalValue = i;
                    return true;
                case long l:
                    decimalValue = l;
                    return true;
                case short s:
                    decimalValue = s;
                    return true;
                case double db:
                    decimalValue = Convert.ToDecimal(db, CultureInfo.InvariantCulture);
                    return true;
                case float f:
                    decimalValue = Convert.ToDecimal(f, CultureInfo.InvariantCulture);
                    return true;
                default:
                    decimalValue = default;
                    return false;
            }
        }

        private static bool TryConvertToInt(object? value, out int intValue)
        {
            switch (value)
            {
                case int i:
                    intValue = i;
                    return true;
                default:
                    intValue = default;
                    return false;
            }
        }

        // Ho tro bo qua validate tiep theo khi string chi gom khoang trang.
        private static bool IsWhitespaceString(object value)
            => value is string text && string.IsNullOrWhiteSpace(text);

        private static bool IsEnumType(Type type)
        {
            var targetType = Nullable.GetUnderlyingType(type) ?? type;
            return targetType.IsEnum;
        }

        private static bool IsEnumerableOfEnum(Type type)
        {
            if (type == typeof(string))
            {
                return false;
            }

            if (!typeof(IEnumerable).IsAssignableFrom(type))
            {
                return false;
            }

            var elementType = GetEnumerableElementType(type);
            return elementType is not null && IsEnumType(elementType);
        }

        private static Type? GetEnumerableElementType(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }

            if (type.IsGenericType)
            {
                return type.GetGenericArguments().FirstOrDefault();
            }

            var enumerableInterface = type
                .GetInterfaces()
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            return enumerableInterface?.GetGenericArguments().FirstOrDefault();
        }
    }
}
