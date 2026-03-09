using AISEP.DTOs.Requests;
using FluentValidation;

namespace AISEP.Validators.Startup
{
    public class UpdateStartupRequestValidator : AbstractValidator<UpdateStartupRequest>
    {
        public UpdateStartupRequestValidator()
        {
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name is required.")
                .MaximumLength(255).WithMessage("Company name must not exceed 255 characters.");

            RuleFor(x => x.LogoUrl)
                .MaximumLength(255).WithMessage("Logo URL must not exceed 255 characters.")
                .When(x => x.LogoUrl is not null);

            RuleFor(x => x.Founder)
                .MaximumLength(255).WithMessage("Founder must not exceed 255 characters.")
                .When(x => x.Founder is not null);

            RuleFor(x => x.CountryCity)
                .MaximumLength(255).WithMessage("Country/City must not exceed 255 characters.")
                .When(x => x.CountryCity is not null);

            RuleFor(x => x.Website)
                .MaximumLength(255).WithMessage("Website must not exceed 255 characters.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Website must be a valid URL.")
                .When(x => !string.IsNullOrEmpty(x.Website));

            RuleFor(x => x.BusinessLicenseUrl)
                .MaximumLength(255).WithMessage("Business license URL must not exceed 255 characters.")
                .When(x => x.BusinessLicenseUrl is not null);
        }
    }
}
