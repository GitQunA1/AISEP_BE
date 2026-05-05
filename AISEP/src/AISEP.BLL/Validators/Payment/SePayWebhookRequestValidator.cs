using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Payment
{
    public class SePayWebhookRequestValidator : AbstractValidator<SePayWebhookRequest>
    {
        public SePayWebhookRequestValidator()
        {
            RuleFor(x => x.TransferAmount)
                .GreaterThan(0).WithMessage("TransferAmount phải lớn hơn 0.");

            RuleFor(x => x.Content)
                .MaximumLength(2000).WithMessage("Content không được vượt quá 2000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Content));

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description không được vượt quá 2000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}
