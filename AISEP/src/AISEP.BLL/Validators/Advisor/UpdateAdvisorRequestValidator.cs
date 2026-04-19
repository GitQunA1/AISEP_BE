using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Advisor
{
    public class UpdateAdvisorRequestValidator : AbstractValidator<UpdateAdvisorRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s.,!?'-]*$";
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
        private static readonly string[] AllowedDocTypes = ["application/pdf", "image/jpeg", "image/png"];
        private const long MaxImageSize = 5 * 1024 * 1024;
        private const long MaxDocSize = 10 * 1024 * 1024;

        public UpdateAdvisorRequestValidator()
        {
            RuleFor(x => x)
                .Must(HasAtLeastOneField)
                .WithMessage("At least one field must be provided for update.");

            RuleFor(x => x.HourlyRate)
                .GreaterThan(0m).WithMessage("Hourly rate must be greater than 0.")
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

            RuleFor(x => x.Bio)
                .NotEmpty().WithMessage("Bio must not be empty when provided.")
                .Matches(TextPattern).WithMessage("Bio contains invalid characters.")
                .When(x => x.Bio is not null);

            RuleFor(x => x.Expertise)
                .NotEmpty().WithMessage("Expertise must not be empty when provided.")
                .Matches(TextPattern).WithMessage("Expertise contains invalid characters.")
                .When(x => x.Expertise is not null);

            RuleFor(x => x.PreviousExperience)
                .NotEmpty().WithMessage("Previous experience must not be empty when provided.")
                .Matches(TextPattern).WithMessage("Previous experience contains invalid characters.")
                .When(x => x.PreviousExperience is not null);

            RuleFor(x => x.LanguagesSpoken)
                .NotEmpty().WithMessage("Languages spoken must not be empty when provided.")
                .Matches(TextPattern).WithMessage("Languages spoken contains invalid characters.")
                .When(x => x.LanguagesSpoken is not null);

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Location must not be empty when provided.")
                .Matches(TextPattern).WithMessage("Location contains invalid characters.")
                .When(x => x.Location is not null);

            RuleForEach(x => x.Industries)
                .IsInEnum().WithMessage("One or more industries are invalid.")
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
