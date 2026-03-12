using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Advisor
{
    public class CreateAdvisorRequestValidator : AbstractValidator<CreateAdvisorRequest>
    {
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
        private static readonly string[] AllowedDocTypes   = ["application/pdf", "image/jpeg", "image/png"];
        private const long MaxImageSize = 5  * 1024 * 1024;  // 5 MB
        private const long MaxDocSize   = 10 * 1024 * 1024;  // 10 MB

        public CreateAdvisorRequestValidator()
        {
            RuleFor(x => x.HourlyRate)
                .GreaterThan(0).WithMessage("Hourly rate must be greater than 0.")
                .When(x => x.HourlyRate.HasValue);

            RuleFor(x => x.ProfileImageFile)
                .Must(f => f!.Length <= MaxImageSize)
                    .WithMessage("Profile image must not exceed 5MB.")
                .Must(f => AllowedImageTypes.Contains(f!.ContentType))
                    .WithMessage("Profile image only supports JPG, PNG, WEBP.")
                .When(x => x.ProfileImageFile is not null);

            RuleFor(x => x.CertificationFile)
                .Must(f => f!.Length <= MaxDocSize)
                    .WithMessage("Certification file must not exceed 10MB.")
                .Must(f => AllowedDocTypes.Contains(f!.ContentType))
                    .WithMessage("Certification only supports PDF, JPG, PNG.")
                .When(x => x.CertificationFile is not null);
        }
    }
}
