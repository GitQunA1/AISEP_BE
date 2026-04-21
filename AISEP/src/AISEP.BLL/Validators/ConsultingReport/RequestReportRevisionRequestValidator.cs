using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.ConsultingReport
{
    public class RequestReportRevisionRequestValidator : AbstractValidator<RequestReportRevisionRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s.,;:!?&()%'""-]*$";

        public RequestReportRevisionRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Lý do là bắt buộc.")
                .MinimumLength(3).WithMessage("Lý do phải có ít nhất 3 ký tự.")
                .MaximumLength(2000).WithMessage("Lý do không được vượt quá 2000 ký tự.")
                .Matches(TextPattern).WithMessage("Lý do chứa ký tự không hợp lệ.");
        }
    }
}
