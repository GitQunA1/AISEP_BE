using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.UserReport
{
    public class ResolveUserReportRequestValidator : AbstractValidator<ResolveUserReportRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s.,;:!?&()%'""-]*$";

        public ResolveUserReportRequestValidator()
        {
            RuleFor(x => x.ResolutionNote)
                .MaximumLength(1000)
                .WithMessage("Ghi chú xử lý không được vượt quá 1000 ký tự.")
                .Matches(TextPattern).WithMessage("Ghi chú xử lý chứa ký tự không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.ResolutionNote));
        }
    }
}
