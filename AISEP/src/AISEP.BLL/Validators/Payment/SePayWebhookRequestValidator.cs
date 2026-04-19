using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Payment
{
    public class SePayWebhookRequestValidator : AbstractValidator<SePayWebhookRequest>
    {
        public SePayWebhookRequestValidator()
        {
            RuleFor(x => x.TransferAmount)
                .GreaterThan(0).WithMessage("TransferAmount must be greater than 0.");

            RuleFor(x => x.Content)
                .MaximumLength(2000).WithMessage("Content must not exceed 2000 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Content));

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}
