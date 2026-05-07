using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Payout
{
    public class MarkPayoutPaidRequestValidator : AbstractValidator<MarkPayoutPaidRequest>
    {
        public MarkPayoutPaidRequestValidator()
        {
            RuleFor(x => x.Note)
                .MaximumLength(1000).WithMessage("Ghi chú không được vượt quá 1000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Note));

            RuleFor(x => x.ProofFile)
                .NotNull().WithMessage("Vui lòng upload chứng từ thanh toán.")
                .Must(file => file is not null && file.Length > 0)
                .WithMessage("Chứng từ thanh toán không được để trống.")
                .Must(BeAllowedProofFile)
                .WithMessage("Chứng từ thanh toán chỉ hỗ trợ PDF, JPG hoặc PNG.")
                .Must(file => file is null || file.Length <= 10 * 1024 * 1024)
                .WithMessage("Chứng từ thanh toán không được vượt quá 10MB.");
        }

        private static bool BeAllowedProofFile(IFormFile? file)
        {
            if (file is null)
            {
                return false;
            }

            var contentType = file.ContentType?.Trim().ToLowerInvariant();
            var extension = Path.GetExtension(file.FileName).Trim().ToLowerInvariant();

            return (contentType, extension) switch
            {
                ("application/pdf", ".pdf") => true,
                ("image/jpeg", ".jpg") => true,
                ("image/jpeg", ".jpeg") => true,
                ("image/png", ".png") => true,
                _ => false
            };
        }
    }
}
