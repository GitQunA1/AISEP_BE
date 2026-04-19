using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Project
{
    public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s]*$";
        private const string TeamMembersPattern = @"^[\p{L}\p{N}\s()]*$";
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
        private const long MaxImageSize = 5 * 1024 * 1024;

        public UpdateProjectRequestValidator()
        {
            RuleFor(x => x)
                .Must(HasAtLeastOneField)
                .WithMessage("At least one field must be provided for update.");

            RuleFor(x => x.ProjectName)
                .NotEmpty().WithMessage("Project name must not be empty when provided.")
                .MaximumLength(255).WithMessage("Project name must not exceed 255 characters.")
                .Matches(TextPattern).WithMessage("Project name contains invalid characters.")
                .When(x => x.ProjectName is not null);

            RuleFor(x => x.ProjectImageFile)
                .Must(f => f!.Length <= MaxImageSize)
                .WithMessage("Project image must not exceed 5MB.")
                .Must(f => AllowedImageTypes.Contains(f!.ContentType))
                .WithMessage("Project image only supports JPG, PNG, WEBP.")
                .When(x => x.ProjectImageFile is not null);

            RuleFor(x => x.DevelopmentStage)
                .IsInEnum().WithMessage("Development stage is not valid. Allowed: Idea, MVP, Growth.")
                .When(x => x.DevelopmentStage.HasValue);

            RuleFor(x => x.Industry)
                .IsInEnum().WithMessage("Industry is invalid.")
                .When(x => x.Industry.HasValue);

            RuleFor(x => x.ShortDescription)
                .NotEmpty().WithMessage("Short description must not be empty when provided.")
                .MaximumLength(500).WithMessage("Short description must not exceed 500 characters.")
                .Matches(TextPattern).WithMessage("Short description contains invalid characters.")
                .When(x => x.ShortDescription is not null);

            RuleFor(x => x.ProblemStatement)
                .NotEmpty().WithMessage("Problem statement must not be empty when provided.")
                .MaximumLength(2000).WithMessage("Problem statement must not exceed 2000 characters.")
                .Matches(TextPattern).WithMessage("Problem statement contains invalid characters.")
                .When(x => x.ProblemStatement is not null);

            RuleFor(x => x.SolutionDescription)
                .NotEmpty().WithMessage("Solution description must not be empty when provided.")
                .MaximumLength(2000).WithMessage("Solution description must not exceed 2000 characters.")
                .Matches(TextPattern).WithMessage("Solution description contains invalid characters.")
                .When(x => x.SolutionDescription is not null);

            RuleFor(x => x.TargetCustomers)
                .NotEmpty().WithMessage("Target customers must not be empty when provided.")
                .MaximumLength(1000).WithMessage("Target customers must not exceed 1000 characters.")
                .Matches(TextPattern).WithMessage("Target customers contains invalid characters.")
                .When(x => x.TargetCustomers is not null);

            RuleFor(x => x.UniqueValueProposition)
                .NotEmpty().WithMessage("Unique value proposition must not be empty when provided.")
                .MaximumLength(1000).WithMessage("Unique value proposition must not exceed 1000 characters.")
                .Matches(TextPattern).WithMessage("Unique value proposition contains invalid characters.")
                .When(x => x.UniqueValueProposition is not null);

            RuleFor(x => x.MarketSize)
                .GreaterThanOrEqualTo(0).WithMessage("Market size must be a positive number.")
                .When(x => x.MarketSize.HasValue);

            RuleFor(x => x.BusinessModel)
                .NotEmpty().WithMessage("Business model must not be empty when provided.")
                .MaximumLength(1000).WithMessage("Business model must not exceed 1000 characters.")
                .Matches(TextPattern).WithMessage("Business model contains invalid characters.")
                .When(x => x.BusinessModel is not null);

            RuleFor(x => x.Revenue)
                .GreaterThanOrEqualTo(0).WithMessage("Revenue must be a positive number.")
                .When(x => x.Revenue.HasValue);

            RuleFor(x => x.Competitors)
                .NotEmpty().WithMessage("Competitors must not be empty when provided.")
                .MaximumLength(1000).WithMessage("Competitors must not exceed 1000 characters.")
                .Matches(TextPattern).WithMessage("Competitors contain invalid characters.")
                .When(x => x.Competitors is not null);

            RuleFor(x => x.TeamMembers)
                .NotEmpty().WithMessage("Team members must not be empty when provided.")
                .MaximumLength(1000).WithMessage("Team members must not exceed 1000 characters.")
                .Matches(TeamMembersPattern).WithMessage("Team members contain invalid characters.")
                .When(x => x.TeamMembers is not null);

            RuleFor(x => x.KeySkills)
                .NotEmpty().WithMessage("Key skills must not be empty when provided.")
                .MaximumLength(1000).WithMessage("Key skills must not exceed 1000 characters.")
                .Matches(TextPattern).WithMessage("Key skills contain invalid characters.")
                .When(x => x.KeySkills is not null);

            RuleFor(x => x.TeamExperience)
                .NotEmpty().WithMessage("Team experience must not be empty when provided.")
                .MaximumLength(1000).WithMessage("Team experience must not exceed 1000 characters.")
                .Matches(TextPattern).WithMessage("Team experience contains invalid characters.")
                .When(x => x.TeamExperience is not null);
        }

        private static bool HasAtLeastOneField(UpdateProjectRequest request)
        {
            return request.ProjectName is not null
                || request.ProjectImageFile is not null
                || request.ShortDescription is not null
                || request.DevelopmentStage.HasValue
                || request.ProblemStatement is not null
                || request.SolutionDescription is not null
                || request.TargetCustomers is not null
                || request.UniqueValueProposition is not null
                || request.MarketSize.HasValue
                || request.BusinessModel is not null
                || request.Revenue.HasValue
                || request.Competitors is not null
                || request.TeamMembers is not null
                || request.KeySkills is not null
                || request.TeamExperience is not null
                || request.Industry.HasValue;
        }
    }
}
