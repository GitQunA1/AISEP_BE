using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.FormValidationRules
{
    public class CreateFormValidationRuleRequestValidator : AbstractValidator<CreateFormValidationRuleRequest>
    {
        public CreateFormValidationRuleRequestValidator()
        {
            Include(new UpsertFormValidationRuleRequestValidator());

            RuleFor(x => x.FormKey)
                .NotEmpty()
                .WithMessage("FormKey is required.");

            RuleFor(x => x.FieldKey)
                .NotEmpty()
                .WithMessage("FieldKey is required.");
        }
    }
}
