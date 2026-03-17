using AISEP.BLL.DTOs.Requests;
using AISEP.DAL.Enums;
using FluentValidation;

namespace AISEP.BLL.Validators.Project
{
    public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
    {
       
            public CreateProjectRequestValidator()
            {
                // Kiểm tra cho giai đoạn Idea - Các trường bắt buộc
                RuleFor(x => x.ProjectName)
                    .NotEmpty().WithMessage("Project name is required.").When(x => x.DevelopmentStage == DevelopmentStage.Idea)
                    .MaximumLength(255).WithMessage("Project name must not exceed 255 characters.")
                    .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$")
                    .WithMessage("Project name contains invalid characters.");
                    

                RuleFor(x => x.ShortDescription)
                    .NotEmpty().WithMessage("Short description is required.")
                    .MaximumLength(500).WithMessage("Short description must not exceed 500 characters.")
                    .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$")
                    .WithMessage("Short description contains invalid characters.")
                    .When(x => x.DevelopmentStage == DevelopmentStage.Idea);

                RuleFor(x => x.ProblemStatement)
                    .NotEmpty().WithMessage("Problem statement is required.")
                    .MaximumLength(2000).WithMessage("Problem statement must not exceed 2000 characters.")
                    .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$")
                    .WithMessage("Problem statement contains invalid characters.")
                    .When(x => x.DevelopmentStage == DevelopmentStage.Idea);

                RuleFor(x => x.SolutionDescription)
                    .NotEmpty().WithMessage("Solution description is required.")
                    .MaximumLength(2000).WithMessage("Solution description must not exceed 2000 characters.")
                    .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$")
                    .WithMessage("Solution description contains invalid characters.")
                    .When(x => x.DevelopmentStage == DevelopmentStage.Idea);

                RuleFor(x => x.TargetCustomers)
                    .NotEmpty().WithMessage("Target customers is required.")
                    .MaximumLength(1000).WithMessage("Target customers must not exceed 1000 characters.")
                    .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$")
                    .WithMessage("Target customers contains invalid characters.")
                    .When(x => x.DevelopmentStage == DevelopmentStage.Idea);

                RuleFor(x => x.TeamMembers)
                    .NotEmpty().WithMessage("Team members is required.")
                    .MaximumLength(1000).WithMessage("Team members must not exceed 1000 characters.")
                    .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$")
                    .WithMessage("Team members contains invalid characters.")
                    .When(x => x.DevelopmentStage == DevelopmentStage.Idea);

            // Kiểm tra cho giai đoạn MVP - Các trường bắt buộc
            RuleFor(x => x.UniqueValueProposition)
                .NotEmpty().WithMessage("Unique value proposition is required.").When(x => x.DevelopmentStage == DevelopmentStage.MVP)
                .MaximumLength(1000).WithMessage("Unique value proposition must not exceed 1000 characters.")
                .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$")
                .WithMessage("Unique value proposition contains invalid characters.");
                    

                RuleFor(x => x.BusinessModel)
                    .NotEmpty().WithMessage("Business model is required.")
                    .MaximumLength(1000).WithMessage("Business model must not exceed 1000 characters.")
                    .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$")
                    .WithMessage("Business model contains invalid characters.")
                    .When(x => x.DevelopmentStage == DevelopmentStage.MVP);

                RuleFor(x => x.KeySkills)
                    .NotEmpty().WithMessage("Key skills are required.")
                    .MaximumLength(1000).WithMessage("Key skills must not exceed 1000 characters.")
                    .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$")
                    .WithMessage("Key skills contains invalid characters.")
                    .When(x => x.DevelopmentStage == DevelopmentStage.MVP);

                RuleFor(x => x.Competitors)
                    .NotEmpty().WithMessage("Competitors are required.")
                    .MaximumLength(1000).WithMessage("Competitors must not exceed 1000 characters.")
                    .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$")
                    .WithMessage("Competitors contains invalid characters.")
                    .When(x => x.DevelopmentStage == DevelopmentStage.MVP);

                // Kiểm tra cho giai đoạn Growth - Các trường bắt buộc
                RuleFor(x => x.Revenue)
                    .NotNull().WithMessage("Revenue is required.")
                    .When(x => x.DevelopmentStage == DevelopmentStage.Growth);

                RuleFor(x => x.MarketSize)
                    .NotNull().WithMessage("Market size is required.")
                    .When(x => x.DevelopmentStage == DevelopmentStage.Growth);

                RuleFor(x => x.TeamExperience)
                    .NotEmpty().WithMessage("Team experience is required.")
                    .When(x => x.DevelopmentStage == DevelopmentStage.Growth);

                RuleFor(x => x.KeySkills)
                    .NotEmpty().WithMessage("Key skills are required.")
                    .When(x => x.DevelopmentStage == DevelopmentStage.Growth);
            }
        }
    }

