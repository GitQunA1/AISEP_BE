using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Project
{
    public class RejectProjectRequestValidator : AbstractValidator<RejectProjectRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s.,;:!?&()%'""-]*$";

        public RejectProjectRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Lý do là bắt buộc khi từ chối dự án.")
                .MaximumLength(2000).WithMessage("Lý do không được vượt quá 2000 ký tự.")
                .Matches(TextPattern).WithMessage("Lý do chứa ký tự không hợp lệ.");
        }
    }
}
