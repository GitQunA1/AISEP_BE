using AISEP.BLL.DTOs.Requests;
using AISEP.DAL.Enums;
using FluentValidation;

namespace AISEP.BLL.Validators.Startup
{
    public class CreateStartupRequestValidator : AbstractValidator<CreateStartupRequest>
    {
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
        private static readonly string[] AllowedDocTypes   = ["application/pdf", "image/jpeg", "image/png"];
        private const long MaxImageSize = 5  * 1024 * 1024;  // 5 MB
        private const long MaxDocSize   = 10 * 1024 * 1024;  // 10 MB

        public CreateStartupRequestValidator()
        {
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name is required.")
                .MaximumLength(255).WithMessage("Company name must not exceed 255 characters.");

            RuleFor(x => x.Founder)
                .MaximumLength(255).WithMessage("Founder must not exceed 255 characters.")
                .When(x => x.Founder is not null);

            RuleFor(x => x.ContactInfo)
                .MaximumLength(500).WithMessage("Contact info must not exceed 500 characters.")
                .When(x => x.ContactInfo is not null);

            RuleFor(x => x.CountryCity)
                .MaximumLength(255).WithMessage("Country/City must not exceed 255 characters.")
                .When(x => x.CountryCity is not null);

            RuleFor(x => x.Website)
                .MaximumLength(255).WithMessage("Website must not exceed 255 characters.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Website must be a valid URL.")
                .When(x => !string.IsNullOrEmpty(x.Website));

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
    }
}
