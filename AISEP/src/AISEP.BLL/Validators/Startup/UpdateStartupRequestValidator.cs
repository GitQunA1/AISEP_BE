using AISEP.BLL.DTOs.Requests;
using AISEP.DAL.Enums;
using FluentValidation;

namespace AISEP.BLL.Validators.Startup
{
    public class UpdateStartupRequestValidator : AbstractValidator<UpdateStartupRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s.,;:!?&()%'""-]*$";
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
        private static readonly string[] AllowedDocTypes = ["application/pdf", "image/jpeg", "image/png"];
        private const long MaxImageSize = 5 * 1024 * 1024;
        private const long MaxDocSize = 10 * 1024 * 1024;

        public UpdateStartupRequestValidator()
        {
            RuleFor(x => x)
                .Must(HasAtLeastOneField)
                .WithMessage("At least one field must be provided for update.");

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name must not be empty when provided.")
                .MaximumLength(255).WithMessage("Company name must not exceed 255 characters.")
                .Matches(TextPattern).WithMessage("Company name contains invalid characters.")
                .When(x => x.CompanyName is not null);

            RuleFor(x => x.Founder)
                .NotEmpty().WithMessage("Founder must not be empty when provided.")
                .MaximumLength(255).WithMessage("Founder must not exceed 255 characters.")
                .Matches(TextPattern).WithMessage("Founder contains invalid characters.")
                .When(x => x.Founder is not null);

            RuleFor(x => x.CountryCity)
                .NotEmpty().WithMessage("Country/City must not be empty when provided.")
                .MaximumLength(255).WithMessage("Country/City must not exceed 255 characters.")
                .Matches(TextPattern).WithMessage("Country/City contains invalid characters.")
                .When(x => x.CountryCity is not null);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email must not be empty when provided.")
                .MaximumLength(255).WithMessage("Email must not exceed 255 characters.")
                .EmailAddress().WithMessage("Email must be a valid email address.")
                .When(x => x.Email is not null);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number must not be empty when provided.")
                .MaximumLength(50).WithMessage("Phone number must not exceed 50 characters.")
                .Matches("^(03|05|07|08|09)\\d{8}$").WithMessage("Phone number must start with 03, 05, 07, 08, or 09 and contain 10 digits.")
                .When(x => x.PhoneNumber is not null);

            RuleFor(x => x.Website)
                .NotEmpty().WithMessage("Website must not be empty when provided.")
                .MaximumLength(255).WithMessage("Website must not exceed 255 characters.")
                .When(x => x.Website is not null);

            RuleFor(x => x.Industry)
                .IsInEnum().WithMessage("Industry is not valid. Allowed: Fintech, Edtech, Healthtech, Agritech, E_Commerce, Logistics, Proptech, Cleantech, SaaS, AI_BigData, Web3_Crypto, Food_Beverage, Manufacturing, Media_Entertainment, Other.")
                .When(x => x.Industry.HasValue);

            RuleFor(x => x.LogoFile)
                .Must(f => f!.Length <= MaxImageSize)
                .WithMessage("Logo must not exceed 5MB.")
                .Must(f => AllowedImageTypes.Contains(f!.ContentType))
                .WithMessage("Logo only supports JPG, PNG, WEBP.")
                .When(x => x.LogoFile is not null);

            RuleFor(x => x.BusinessLicenseFile)
                .Must(f => f!.Length <= MaxDocSize)
                .WithMessage("Business license must not exceed 10MB.")
                .Must(f => AllowedDocTypes.Contains(f!.ContentType))
                .WithMessage("Business license only supports PDF, JPG, PNG.")
                .When(x => x.BusinessLicenseFile is not null);
        }

        private static bool HasAtLeastOneField(UpdateStartupRequest request)
        {
            return request.CompanyName is not null
                || request.Founder is not null
                || request.Email is not null
                || request.PhoneNumber is not null
                || request.CountryCity is not null
                || request.Website is not null
                || request.Industry.HasValue
                || request.LogoFile is not null
                || request.BusinessLicenseFile is not null;
        }
    }
}
