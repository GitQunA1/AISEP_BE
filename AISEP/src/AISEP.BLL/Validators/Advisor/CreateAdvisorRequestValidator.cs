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
                .NotEmpty().WithMessage("Hourly rate is required.")
                .GreaterThan(0m).WithMessage("Hourly rate must be greater than 0.");

            RuleFor(x => x.ProfileImageFile)
                .NotEmpty().WithMessage("Profile image file is required.")
                .Must(f => f!.Length <= MaxImageSize)
                .WithMessage("Profile image must not exceed 5MB.")
                .Must(f => AllowedImageTypes.Contains(f!.ContentType))
                .WithMessage("Profile image only supports JPG, PNG, WEBP.");

            RuleFor(x => x.CertificationFile)
                .NotEmpty().WithMessage("Certification file is required.")
                .Must(f => f!.Length <= MaxDocSize)
                .WithMessage("Certification file must not exceed 10MB.")
                .Must(f => AllowedDocTypes.Contains(f!.ContentType))
                .WithMessage("Certification only supports PDF, JPG, PNG.");

            RuleFor(x => x.Bio)
                .NotEmpty().WithMessage("Bio is required.")
                .Matches(TextPattern).WithMessage("Bio contains invalid characters.");

            RuleFor(x => x.Expertise)
                .NotEmpty().WithMessage("Expertise is required.")
                .Matches(TextPattern).WithMessage("Expertise contains invalid characters.");

            RuleFor(x => x.PreviousExperience)
                .NotEmpty().WithMessage("Previous experience is required.")
                .Matches(TextPattern).WithMessage("Previous experience contains invalid characters.");

            RuleFor(x => x.LanguagesSpoken)
                .NotEmpty().WithMessage("Languages spoken is required.")
                .Matches(TextPattern).WithMessage("Languages spoken contains invalid characters.");

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Location is required.")
                .Matches(TextPattern).WithMessage("Location contains invalid characters.");

            RuleForEach(x => x.Industries)
                .IsInEnum().WithMessage("One or more industries are invalid.")
                .When(x => x.Industries is not null);

            RuleFor(x => x.Industries)
                .NotNull().WithMessage("Industries is required.")
                .Must(x => x is { Count: > 0 }).WithMessage("At least one industry is required.");
        }
    }
}
