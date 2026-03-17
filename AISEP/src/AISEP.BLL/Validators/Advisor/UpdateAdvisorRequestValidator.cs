using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Advisor
{
    public class UpdateAdvisorRequestValidator : AbstractValidator<UpdateAdvisorRequest>
    {
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
        private static readonly string[] AllowedDocTypes   = ["application/pdf", "image/jpeg", "image/png"];
        private const long MaxImageSize = 5  * 1024 * 1024;  // 5 MB
        private const long MaxDocSize   = 10 * 1024 * 1024;  // 10 MB

        public UpdateAdvisorRequestValidator()
        {
            RuleFor(x => x.HourlyRate)
                .GreaterThan(0m).WithMessage("Hourly rate must be greater than 0.")
                .When(x => x.HourlyRate is not null && x.HourlyRate != 0);

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
                .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$").WithMessage("Bio contains invalid characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Bio));

            RuleFor(x => x.Expertise)
                .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$").WithMessage("Expertise contains invalid characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Expertise));

            RuleFor(x => x.PreviousExperience)
               .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$").WithMessage("Previous experience contains invalid characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.PreviousExperience));

            RuleFor(x => x.LanguagesSpoken)
               .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$").WithMessage("Languages spoken must not contain numbers or special characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.LanguagesSpoken));

            RuleFor(x => x.Location)
                .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$").WithMessage("Location must not contain numbers or special characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Location));
        }
    }
}
