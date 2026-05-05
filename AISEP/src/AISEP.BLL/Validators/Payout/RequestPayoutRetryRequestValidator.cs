using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Payout
{
    public class RequestPayoutRetryRequestValidator : AbstractValidator<RequestPayoutRetryRequest>
    {
        public RequestPayoutRetryRequestValidator()
        {
            RuleFor(x => x.ResolutionNote)
                .NotEmpty().WithMessage("Ghi chú xử lý là bắt buộc.")
                .MaximumLength(1000).WithMessage("Ghi chú xử lý không được vượt quá 1000 ký tự.");
        }
    }
}
