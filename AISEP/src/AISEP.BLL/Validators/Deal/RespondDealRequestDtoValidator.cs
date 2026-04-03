using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Deal
{
    public class RespondDealRequestDtoValidator : AbstractValidator<RespondDealRequestDto>
    {
        public RespondDealRequestDtoValidator()
        {
            RuleFor(x => x.IsAccepted)
                .NotNull().WithMessage("IsAccepted is required.");
        }
    }
}
