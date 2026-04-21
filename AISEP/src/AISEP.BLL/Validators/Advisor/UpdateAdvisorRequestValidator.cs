using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Advisor
{
    public class UpdateAdvisorRequestValidator : AbstractValidator<UpdateAdvisorRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s.,;:!?&()%'""-]*$";
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
        private static readonly string[] AllowedDocTypes = ["application/pdf", "image/jpeg", "image/png"];
        private const long MaxImageSize = 5 * 1024 * 1024;
        private const long MaxDocSize = 10 * 1024 * 1024;

        public UpdateAdvisorRequestValidator()
        {
            RuleFor(x => x)
                .Must(HasAtLeastOneField)
                .WithMessage("Cần cung cấp ít nhất một trường để cập nhật.");

            RuleFor(x => x.HourlyRate)
                .GreaterThan(0m).WithMessage("Mức phí theo giờ phải lớn hơn 0.")
                .When(x => x.HourlyRate.HasValue);

            RuleFor(x => x.ProfileImageFile)
                .Must(f => f!.Length <= MaxImageSize)
                .WithMessage("Ảnh đại diện không được vượt quá 5MB.")
                .Must(f => AllowedImageTypes.Contains(f!.ContentType))
                .WithMessage("Ảnh đại diện chỉ hỗ trợ JPG, PNG, WEBP.")
                .When(x => x.ProfileImageFile is not null);

            RuleFor(x => x.CertificationFile)
                .Must(f => f!.Length <= MaxDocSize)
                .WithMessage("Tệp chứng chỉ không được vượt quá 10MB.")
                .Must(f => AllowedDocTypes.Contains(f!.ContentType))
                .WithMessage("Chứng chỉ chỉ hỗ trợ PDF, JPG, PNG.")
                .When(x => x.CertificationFile is not null);

            RuleFor(x => x.Bio)
                .NotEmpty().WithMessage("Tiểu sử không được để trống khi được cung cấp.")
                .Matches(TextPattern).WithMessage("Tiểu sử chứa ký tự không hợp lệ.")
                .When(x => x.Bio is not null);

            RuleFor(x => x.Expertise)
                .NotEmpty().WithMessage("Chuyên môn không được để trống khi được cung cấp.")
                .Matches(TextPattern).WithMessage("Chuyên môn chứa ký tự không hợp lệ.")
                .When(x => x.Expertise is not null);

            RuleFor(x => x.PreviousExperience)
                .NotEmpty().WithMessage("Kinh nghiệm trước đây không được để trống khi được cung cấp.")
                .Matches(TextPattern).WithMessage("Kinh nghiệm trước đây chứa ký tự không hợp lệ.")
                .When(x => x.PreviousExperience is not null);

            RuleFor(x => x.LanguagesSpoken)
                .NotEmpty().WithMessage("Ngôn ngữ sử dụng không được để trống khi được cung cấp.")
                .Matches(TextPattern).WithMessage("Ngôn ngữ sử dụng chứa ký tự không hợp lệ.")
                .When(x => x.LanguagesSpoken is not null);

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Địa điểm không được để trống khi được cung cấp.")
                .Matches(TextPattern).WithMessage("Địa điểm chứa ký tự không hợp lệ.")
                .When(x => x.Location is not null);

            RuleForEach(x => x.Industries)
                .IsInEnum().WithMessage("Một hoặc nhiều ngành không hợp lệ.")
                .When(x => x.Industries is not null);
        }

        private static bool HasAtLeastOneField(UpdateAdvisorRequest request)
        {
            return request.Bio is not null
                || request.Expertise is not null
                || request.Industries is not null
                || request.PreviousExperience is not null
                || request.LanguagesSpoken is not null
                || request.Location is not null
                || request.HourlyRate.HasValue
                || request.ProfileImageFile is not null
                || request.CertificationFile is not null;
        }
    }
}
