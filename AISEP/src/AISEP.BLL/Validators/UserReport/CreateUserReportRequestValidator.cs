using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.UserReport
{
    public class CreateUserReportRequestValidator : AbstractValidator<CreateUserReportRequest>
    {
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
        private const long MaxImageSize = 2 * 1024 * 1024; // 2MB
        private const int MaxImageCount = 3;

        public CreateUserReportRequestValidator()
        {
            RuleFor(x => x.ReportedUserId)
                .GreaterThan(0).WithMessage("ReportedUserId must be a positive number.");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Report category is invalid.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

            RuleFor(x => x.VideoEvidenceUrl)
                .MaximumLength(1000).WithMessage("Video evidence URL must not exceed 1000 characters.")
                .Must(BeValidUrl).WithMessage("Video evidence URL is invalid.")
                .When(x => !string.IsNullOrWhiteSpace(x.VideoEvidenceUrl));

            RuleFor(x => x.EvidenceImages)
                .Must(files => files == null || files.Count <= MaxImageCount)
                .WithMessage("You can upload up to 3 images.");

            RuleForEach(x => x.EvidenceImages)
                .Must(file => file.Length <= MaxImageSize)
                .WithMessage("Each image must not exceed 2MB.")
                .Must(file => AllowedImageTypes.Contains(file.ContentType))
                .WithMessage("Only JPG, PNG, WEBP images are supported.")
                .When(x => x.EvidenceImages is not null && x.EvidenceImages.Count > 0);
        }

        private static bool BeValidUrl(string? url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri)
                   && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
