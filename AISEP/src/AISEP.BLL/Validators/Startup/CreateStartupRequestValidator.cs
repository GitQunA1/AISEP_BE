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
                .MaximumLength(255).WithMessage("Company name must not exceed 255 characters.")
                .Matches("^[a-zA-Z0-9 .,!?'-]*$").WithMessage("Company name contains invalid characters.");

            RuleFor(x => x.Founder)
                .NotEmpty().WithMessage("Founder is required.")
                .MaximumLength(255).WithMessage("Founder must not exceed 255 characters.")
                .Matches("^[a-zA-Z0-9 .,!?'-]*$").WithMessage("Founder contains invalid characters.");
                //.When(x => x.Founder is not null);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .MaximumLength(255).WithMessage("Email must not exceed 255 characters.")
                .EmailAddress().WithMessage("Email must be a valid email address.");
            //.When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(50).WithMessage("Phone number must not exceed 50 characters.")
                .Matches("^(03|05|07|08|09)\\d{8}$").WithMessage("Phone number must start with 03, 05, 07, 08, or 09 and contain 10 digits.");
            //.When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.CountryCity)
                .NotEmpty().WithMessage("Country/City is required.")
                .MaximumLength(255).WithMessage("Country/City must not exceed 255 characters.")
                .Matches("^[a-zA-Z0-9 .,!?'-]*$").WithMessage("Country/City contains invalid characters.");
            //.When(x => x.CountryCity is not null);

            RuleFor(x => x.Website)
                .NotEmpty().WithMessage("Website is required.")
                .MaximumLength(255).WithMessage("Website must not exceed 255 characters.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Website must be a valid URL.");
                //.When(x => !string.IsNullOrWhiteSpace(x.Website));

            RuleFor(x => x.Industry)
                .NotEmpty().WithMessage("Industry is required.")
                .IsInEnum().WithMessage("Industry is not valid. Allowed: Fintech, Edtech, Healthtech, Agritech, E_Commerce, Logistics, Proptech, Cleantech, SaaS, AI_BigData, Web3_Crypto, Food_Beverage, Manufacturing, Media_Entertainment, Other.")
                .When(x => x.Industry.HasValue);

            RuleFor(x => x.LogoFile)
                .NotEmpty().WithMessage("Logo file is required.")
                .Must(f => f!.Length <= MaxImageSize)
                    .WithMessage("Logo must not exceed 5MB.")
                .Must(f => AllowedImageTypes.Contains(f!.ContentType))
                    .WithMessage("Logo only supports JPG, PNG, WEBP.")
                .When(x => x.LogoFile is not null);

            RuleFor(x => x.BusinessLicenseFile)
                .NotEmpty().WithMessage("Business license file is required.")
                .Must(f => f!.Length <= MaxDocSize)
                    .WithMessage("Business license must not exceed 10MB.")
                .Must(f => AllowedDocTypes.Contains(f!.ContentType))
                    .WithMessage("Business license only supports PDF, JPG, PNG.")
                .When(x => x.BusinessLicenseFile is not null);
        }
    }
}
