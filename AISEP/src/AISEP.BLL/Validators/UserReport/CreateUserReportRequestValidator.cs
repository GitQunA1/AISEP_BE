using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.UserReport
{
    public class CreateUserReportRequestValidator : AbstractValidator<CreateUserReportRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s.,;:!?&()%'""-]*$";
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
        private const long MaxImageSize = 2 * 1024 * 1024;
        private const int MaxImageCount = 3;

        public CreateUserReportRequestValidator()
        {
            RuleFor(x => x.BookingId)
                .GreaterThan(0).WithMessage("BookingId phải lớn hơn 0.");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Danh mục báo cáo không hợp lệ.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Mô tả là bắt buộc.")
                .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự.")
                .Matches(TextPattern).WithMessage("Mô tả chứa ký tự không hợp lệ.");

            RuleFor(x => x.VideoEvidenceUrl)
                .MaximumLength(1000).WithMessage("URL video bằng chứng không được vượt quá 1000 ký tự.")
                .Must(BeValidUrl).WithMessage("URL video bằng chứng không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.VideoEvidenceUrl));

            RuleFor(x => x.EvidenceImages)
                .Must(files => files == null || files.Count <= MaxImageCount)
                .WithMessage("Chỉ được tải lên tối đa 3 ảnh.");

            RuleForEach(x => x.EvidenceImages)
                .Must(file => file.Length <= MaxImageSize)
                .WithMessage("Mỗi ảnh không được vượt quá 2MB.")
                .Must(file => AllowedImageTypes.Contains(file.ContentType))
                .WithMessage("Chỉ hỗ trợ ảnh JPG, PNG, WEBP.")
                .When(x => x.EvidenceImages is not null && x.EvidenceImages.Count > 0);
        }

        private static bool BeValidUrl(string? url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri)
                   && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
