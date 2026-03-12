using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Chat
{
    public class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
    {
        public SendMessageRequestValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("The message content cannot be empty.")
                .MaximumLength(2000).WithMessage("The message cannot exceed 2000 characters.");
        }
    }
}
