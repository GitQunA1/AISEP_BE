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
                .NotEmpty().WithMessage("Hourly rate is required.")
                .GreaterThan(0m).WithMessage("Hourly rate must be greater than 0.");
            //.When(x => x.HourlyRate is not null && x.HourlyRate != 0);

            RuleFor(x => x.ProfileImageFile)
                .NotEmpty().WithMessage("Profile image file is required.")
                .Must(f => f!.Length <= MaxImageSize)
                    .WithMessage("Profile image must not exceed 5MB.")
                .Must(f => AllowedImageTypes.Contains(f!.ContentType))
                    .WithMessage("Profile image only supports JPG, PNG, WEBP.");
            //.When(x => x.ProfileImageFile is not null);

            RuleFor(x => x.CertificationFile)
                .NotEmpty().WithMessage("Certification file is required.")
                .Must(f => f!.Length <= MaxDocSize)
                    .WithMessage("Certification file must not exceed 10MB.")
                .Must(f => AllowedDocTypes.Contains(f!.ContentType))
                    .WithMessage("Certification only supports PDF, JPG, PNG.");
               // .When(x => x.CertificationFile is not null);

            RuleFor(x => x.Bio)
                .NotEmpty().WithMessage("Bio is required.")
                .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$").WithMessage("Bio contains invalid characters.");

            RuleFor(x => x.Expertise)
                .NotEmpty().WithMessage("Expertise is required.")
                 .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$").WithMessage("Expertise contains invalid characters.");

            RuleFor(x => x.PreviousExperience)
                .NotEmpty().WithMessage("Previous experience is required.")
                .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$").WithMessage("Previous experience contains invalid characters.");

            RuleFor(x => x.LanguagesSpoken)
                .NotEmpty().WithMessage("Languages spoken is required.")
                .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$").WithMessage("Languages spoken must not contain numbers or special characters.");

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Location is required.")
                 .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$").WithMessage("Location must not contain numbers or special characters.");
        }
    }
}
