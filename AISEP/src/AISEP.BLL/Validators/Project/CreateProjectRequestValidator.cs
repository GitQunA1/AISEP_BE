using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Project
{
    public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
    {
        public CreateProjectRequestValidator()
        {
            RuleFor(x => x.ProjectName)
                .NotEmpty().WithMessage("Project name is required.")
                .MaximumLength(255).WithMessage("Project name must not exceed 255 characters.");

            RuleFor(x => x.ShortDescription)
                .NotEmpty().WithMessage("Short description is required.")
                .MaximumLength(500).WithMessage("Short description must not exceed 500 characters.");

            RuleFor(x => x.DevelopmentStage)
                .IsInEnum().WithMessage("Development stage is required.");

            RuleFor(x => x.ProblemStatement)
                .NotEmpty().WithMessage("Problem statement is required.")
                .MaximumLength(2000).WithMessage("Problem statement must not exceed 2000 characters.");

            RuleFor(x => x.SolutionDescription)
                .NotEmpty().WithMessage("Solution description is required.")
                .MaximumLength(2000).WithMessage("Solution description must not exceed 2000 characters.");

            RuleFor(x => x.TargetCustomers)
                .NotEmpty().WithMessage("Target customers is required.")
                .MaximumLength(1000).WithMessage("Target customers must not exceed 1000 characters.");

            RuleFor(x => x.UniqueValueProposition)
                .NotEmpty().WithMessage("Unique value proposition is required.")
                .MaximumLength(1000).WithMessage("Unique value proposition must not exceed 1000 characters.");

            RuleFor(x => x.MarketSize)
                .NotNull().WithMessage("Market size is required.")
                .GreaterThanOrEqualTo(0).WithMessage("Market size must be a positive number.");

            RuleFor(x => x.BusinessModel)
                .NotEmpty().WithMessage("Business model is required.")
                .MaximumLength(1000).WithMessage("Business model must not exceed 1000 characters.");

            RuleFor(x => x.Revenue)
                .GreaterThanOrEqualTo(0).WithMessage("Revenue must be a positive number.")
                .When(x => x.Revenue.HasValue);

            RuleFor(x => x.Competitors)
                .MaximumLength(1000).WithMessage("Competitors must not exceed 1000 characters.")
                .When(x => x.Competitors is not null);

            RuleFor(x => x.TeamMembers)
                .NotEmpty().WithMessage("Team members is required.")
                .MaximumLength(1000).WithMessage("Team members must not exceed 1000 characters.");

            RuleFor(x => x.KeySkills)
                .MaximumLength(1000).WithMessage("Key skills must not exceed 1000 characters.")
                .When(x => x.KeySkills is not null);

            RuleFor(x => x.TeamExperience)
                .MaximumLength(1000).WithMessage("Team experience must not exceed 1000 characters.")
                .When(x => x.TeamExperience is not null);
        }
    }
}
