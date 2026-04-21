using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Advisor
{
    public class CreateAdvisorRequestValidator : AbstractValidator<CreateAdvisorRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s.,;:!?&()%'""-]*$";
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
        private static readonly string[] AllowedDocTypes = ["application/pdf", "image/jpeg", "image/png"];
        private const long MaxImageSize = 5 * 1024 * 1024;
        private const long MaxDocSize = 10 * 1024 * 1024;

        public CreateAdvisorRequestValidator()
        {
            RuleFor(x => x.HourlyRate)
                .NotEmpty().WithMessage("Mức phí theo giờ là bắt buộc.")
                .GreaterThan(0m).WithMessage("Mức phí theo giờ phải lớn hơn 0.");

            RuleFor(x => x.ProfileImageFile)
                .NotEmpty().WithMessage("Ảnh đại diện là bắt buộc.")
                .Must(f => f!.Length <= MaxImageSize)
                .WithMessage("Ảnh đại diện không được vượt quá 5MB.")
                .Must(f => AllowedImageTypes.Contains(f!.ContentType))
                .WithMessage("Ảnh đại diện chỉ hỗ trợ JPG, PNG, WEBP.");

            RuleFor(x => x.CertificationFile)
                .NotEmpty().WithMessage("Tệp chứng chỉ là bắt buộc.")
                .Must(f => f!.Length <= MaxDocSize)
                .WithMessage("Tệp chứng chỉ không được vượt quá 10MB.")
                .Must(f => AllowedDocTypes.Contains(f!.ContentType))
                .WithMessage("Chứng chỉ chỉ hỗ trợ PDF, JPG, PNG.");

            RuleFor(x => x.Bio)
                .NotEmpty().WithMessage("Tiểu sử là bắt buộc.")
                .Matches(TextPattern).WithMessage("Tiểu sử chứa ký tự không hợp lệ.");

            RuleFor(x => x.Expertise)
                .NotEmpty().WithMessage("Chuyên môn là bắt buộc.")
                .Matches(TextPattern).WithMessage("Chuyên môn chứa ký tự không hợp lệ.");

            RuleFor(x => x.PreviousExperience)
                .NotEmpty().WithMessage("Kinh nghiệm trước đây là bắt buộc.")
                .Matches(TextPattern).WithMessage("Kinh nghiệm trước đây chứa ký tự không hợp lệ.");

            RuleFor(x => x.LanguagesSpoken)
                .NotEmpty().WithMessage("Ngôn ngữ sử dụng là bắt buộc.")
                .Matches(TextPattern).WithMessage("Ngôn ngữ sử dụng chứa ký tự không hợp lệ.");

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Địa điểm là bắt buộc.")
                .Matches(TextPattern).WithMessage("Địa điểm chứa ký tự không hợp lệ.");

            RuleForEach(x => x.Industries)
                .IsInEnum().WithMessage("Một hoặc nhiều ngành không hợp lệ.")
                .When(x => x.Industries is not null);

            RuleFor(x => x.Industries)
                .NotNull().WithMessage("Danh sách ngành là bắt buộc.")
                .Must(x => x is { Count: > 0 }).WithMessage("Cần chọn ít nhất một ngành.");
        }
    }
}
